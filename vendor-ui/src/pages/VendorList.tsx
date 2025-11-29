import React, { useEffect, useState } from "react";
import { useDebouncedValue } from "@mantine/hooks";
import {
  Container,
  Table,
  Button,
  TextInput,
  Group,
  Pagination,
  Loader,
  Center,
  Paper,
  Title,
  Text,
  Stack,
  NumberInput,
  Flex,
  Divider,
  Checkbox,
  MultiSelect,
  List,
} from "@mantine/core";
import { IconSearch, IconRefresh } from "@tabler/icons-react";

import { VendorApi } from "../api/VendorApi";
import { RiskBadge } from "../components/RiskBadge";
import type {
  VendorWithRiskDto,
  PaginatedList,
  Result,
  RiskAssessmentDto,
} from "../types/vendor";

type VendorFormState = {
  id?: string;
  name: string;
  financialHealth: number | "";
  slaUptime: number | "";
  majorIncidents: number | "";
  securityCerts: string[];
  contractValid: boolean;
  privacyPolicyValid: boolean;
  pentestReportValid: boolean;
};

type RiskDetailsState = {
  vendorName: string;
  riskLevel: string;
  scorePercent: string;
  createdAt: string;
  factors: string[];
} | null;

const initialForm: VendorFormState = {
  id: undefined,
  name: "",
  financialHealth: "",
  slaUptime: "",
  majorIncidents: "",
  securityCerts: [],
  contractValid: true,
  privacyPolicyValid: true,
  pentestReportValid: true,
};

// 🔧 reason string'ini parçalayan yardımcı fonksiyon
const parseReasonToFactors = (reason?: string | null): string[] => {
  if (!reason) return [];
  return reason
    .split(" + ")
    .map((p) => p.trim())
    .filter((p) => p.length > 0);
};

const VendorList: React.FC = () => {
  const [vendors, setVendors] = useState<VendorWithRiskDto[]>([]);
  const [pageIndex, setPageIndex] = useState<number>(1);
  const [pageSize, setPageSize] = useState<number>(10);
  const [totalPages, setTotalPages] = useState<number>(1);
  const [totalCount, setTotalCount] = useState<number>(0);
  const [query, setQuery] = useState<string>("");
  const [debouncedQuery] = useDebouncedValue(query, 400);
  const [loading, setLoading] = useState<boolean>(false);
  const [sort] = useState<string | undefined>(undefined);
  const [error, setError] = useState<string | null>(null);
  const [infoMessage, setInfoMessage] = useState<string | null>(null);

  const [form, setForm] = useState<VendorFormState>(initialForm);
  const isEdit = !!form.id;

  // Risk detay paneli için state
  const [riskDetails, setRiskDetails] = useState<RiskDetailsState>(null);

  const loadData = async (page: number, size: number, search: string) => {
    setLoading(true);
    setError(null);
    setInfoMessage(null);

    try {
      const response = await VendorApi.getList({
        pageIndex: page,
        pageSize: size,
        sort,
        query: search || undefined,
      });

      const data = response.data as Result<PaginatedList<VendorWithRiskDto>>;

      if (!data.isSuccess || !data.value) {
        const msg =
          data.errors && data.errors.length > 0
            ? data.errors.join(", ")
            : "Beklenmeyen bir hata oluştu";
        setError(msg);
        return;
      }

      const value = data.value;
      const items = value.items ?? [];
      const count = value.totalCount ?? 0;

      setVendors(items);
      setTotalCount(count);

      const calculatedTotalPages =
        size === -1 ? 1 : Math.max(1, Math.ceil(count / size));

      setTotalPages(calculatedTotalPages);
      setPageIndex(page);
      setPageSize(size);
    } catch (err: unknown) {
      console.error(err);
      setError(
        err instanceof Error ? err.message : "İstek sırasında hata oluştu"
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData(pageIndex, pageSize, debouncedQuery);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pageIndex, pageSize, debouncedQuery]);

  const handleChangePage = (page: number) => {
    setPageIndex(page);
  };

  const handleChangePageSize = (value: number | string) => {
    const parsed =
      typeof value === "number" ? value : parseInt(value as string, 10);
    const newSize = Number.isNaN(parsed) ? 10 : parsed;

    setPageIndex(1);
    setPageSize(newSize);
  };

  const resetForm = () => {
    setForm(initialForm);
  };

  const startEdit = (vendor: VendorWithRiskDto) => {
    // Edit modunda sadece UI'da gördüğün alanları doldur
    setForm({
      ...initialForm,
      id: vendor.id,
      name: vendor.name,
      financialHealth: vendor.financialHealth,
      slaUptime: vendor.slaUptime,
      majorIncidents: vendor.majorIncidents,
    });
  };

  const handleSubmit = async () => {
    setError(null);
    setInfoMessage(null);

    if (
      !form.name ||
      form.financialHealth === "" ||
      form.slaUptime === "" ||
      form.majorIncidents === ""
    ) {
      setError("Lütfen tüm zorunlu alanları doldurun.");
      return;
    }

    try {
      if (isEdit && form.id) {
        // UPDATE: sadece görünen alanlar
        const updatePayload = {
          id: form.id,
          name: form.name.trim(),
          financialHealth: Number(form.financialHealth),
          slaUptime: Number(form.slaUptime),
          majorIncidents: Number(form.majorIncidents),
        };

        const res = await VendorApi.updateVendor(form.id, updatePayload as any);

        if (res.status >= 200 && res.status < 300) {
          setInfoMessage("Vendor başarıyla güncellendi.");
          await loadData(pageIndex, pageSize, debouncedQuery);
        } else {
          setError("Vendor güncellenirken hata oluştu.");
        }
      } else {
        // CREATE: tüm alanlar
        const createPayload = {
          name: form.name.trim(),
          financialHealth: Number(form.financialHealth),
          slaUptime: Number(form.slaUptime),
          majorIncidents: Number(form.majorIncidents),
          securityCerts: form.securityCerts,
          documents: {
            contractValid: form.contractValid,
            privacyPolicyValid: form.privacyPolicyValid,
            pentestReportValid: form.pentestReportValid,
          },
        };

        const res = await VendorApi.createVendor(createPayload);

        if (!res.data.isSuccess) {
          setError(
            res.data.errors?.join(", ") || "Vendor oluşturulurken hata oluştu."
          );
        } else {
          setInfoMessage("Vendor başarıyla oluşturuldu.");
          setPageIndex(1);
          await loadData(1, pageSize, debouncedQuery);
        }
      }

      resetForm();
    } catch (err: unknown) {
      console.error(err);
      setError(
        err instanceof Error ? err.message : "Kayıt sırasında hata oluştu"
      );
    }
  };

  const handleDelete = async (id: string) => {
    setError(null);
    setInfoMessage(null);

    if (!window.confirm("Bu vendor silinsin mi?")) return;

    try {
      const res = await VendorApi.deleteVendor(id);

      if (res.status >= 200 && res.status < 300) {
        setInfoMessage("Vendor başarıyla silindi.");
        await loadData(pageIndex, pageSize, debouncedQuery);
      } else {
        setError("Vendor silinirken hata oluştu.");
      }
    } catch (err: unknown) {
      console.error(err);
      setError(
        err instanceof Error ? err.message : "Silme sırasında hata oluştu"
      );
    }
  };

  // Liste endpoint'inden dönen latestRisk'i detay paneline yansıtmak için
  const showRiskDetailsFromVendor = (vendor: VendorWithRiskDto) => {
    if (!vendor.latestRisk) {
      setRiskDetails(null);
      setInfoMessage("Bu vendor için kayıtlı risk detayı bulunamadı.");
      return;
    }

    const r = vendor.latestRisk;
    const scorePercent = (r.riskScore * 100).toFixed(0);
    const factors = parseReasonToFactors(r.reason);

    setRiskDetails({
      vendorName: vendor.name,
      riskLevel: r.riskLevel,
      scorePercent,
      createdAt: new Date(r.createdAt).toLocaleString(),
      factors,
    });
    setInfoMessage(`"${vendor.name}" için risk detayı görüntüleniyor.`);
  };

  // /Vendor/{id}/risk endpoint'i ile güncel risk hesaplama
  const handleRecalculateRisk = async (id: string) => {
    setError(null);
    setInfoMessage(null);

    const targetVendor = vendors.find((v) => v.id === id);

    try {
      const res = await VendorApi.getRisk(id);

      if (!res.data.isSuccess || !res.data.value) {
        setError(
          res.data.errors?.join(", ") || "Risk hesaplanırken hata oluştu."
        );
        return;
      }

      const risk: RiskAssessmentDto = res.data.value;

      // 1) Tablo satırındaki latestRisk'i güncelle
      setVendors((prev) =>
        prev.map((v) =>
          v.id === id
            ? {
                ...v,
                latestRisk: risk,
              }
            : v
        )
      );

      // 2) Detay paneli için reason'ı parçala
      const scorePercent = (risk.riskScore * 100).toFixed(0);
      const factors = parseReasonToFactors(risk.reason);

      setRiskDetails({
        vendorName: targetVendor?.name ?? "Vendor",
        riskLevel: risk.riskLevel,
        scorePercent,
        createdAt: new Date(risk.createdAt).toLocaleString(),
        factors,
      });

      setInfoMessage(
        `"${targetVendor?.name ?? "Vendor"}" için risk güncellendi.`
      );
    } catch (err: unknown) {
      console.error(err);
      setError(
        err instanceof Error
          ? err.message
          : "Risk hesaplama sırasında hata oluştu"
      );
    }
  };

  const rows = vendors.map((vendor) => (
    <Table.Tr key={vendor.id}>
      <Table.Td>
        <Text fw={500}>{vendor.name}</Text>
      </Table.Td>
      <Table.Td>
        <Text>{vendor.financialHealth}</Text>
      </Table.Td>
      <Table.Td>
        <Text>{vendor.slaUptime.toFixed(2)}%</Text>
      </Table.Td>
      <Table.Td>
        <Text>{vendor.majorIncidents}</Text>
      </Table.Td>
      <Table.Td>
        <Stack gap={2}>
          <RiskBadge risk={vendor.latestRisk ?? null} />
          {vendor.latestRisk?.reason && (
            <Button
              size="xs"
              variant="subtle"
              onClick={() => showRiskDetailsFromVendor(vendor)}
            >
              Detay
            </Button>
          )}
        </Stack>
      </Table.Td>
      <Table.Td>
        <Group gap={4} justify="flex-end">
          <Button size="xs" variant="subtle" onClick={() => startEdit(vendor)}>
            Düzenle
          </Button>
          <Button
            size="xs"
            color="grape"
            variant="subtle"
            onClick={() => handleRecalculateRisk(vendor.id)}
          >
            Riski Hesapla
          </Button>
          <Button
            size="xs"
            color="red"
            variant="subtle"
            onClick={() => handleDelete(vendor.id)}
          >
            Sil
          </Button>
        </Group>
      </Table.Td>
    </Table.Tr>
  ));

  return (
    <Container size="xl" py="lg">
      <Stack gap="md">
        {/* HEADER */}
        <Paper withBorder radius="md" p="md">
          <Flex
            justify="space-between"
            align="flex-start"
            direction={{ base: "column", sm: "row" }}
            gap="sm"
          >
            <div>
              <Title order={2}>Vendor Risk Dashboard</Title>
              <Text c="dimmed" size="sm">
                Tedarikçilerin risk skorlarını yönetin, yeni vendor ekleyin ve
                risk hesaplamalarını tetikleyin.
              </Text>
            </div>

            <Group gap="xs">
              <Text size="sm" c="dimmed">
                Toplam Vendor:
              </Text>
              <Text fw={600}>{totalCount}</Text>
            </Group>
          </Flex>
        </Paper>

        {/* RISK DETAY PANELİ – alt alta okunabilir faktörler */}
        {riskDetails && (
          <Paper
            withBorder
            radius="md"
            p="md"
            style={{ backgroundColor: "rgba(255, 243, 205, 0.7)" }}
          >
            <Text fw={500}>
              "{riskDetails.vendorName}" için güncel risk:{" "}
              <Text span fw={700}>
                {riskDetails.riskLevel} (skor: {riskDetails.scorePercent})
              </Text>
            </Text>
            <Text size="xs" c="dimmed" mt={4}>
              Oluşturulma zamanı: {riskDetails.createdAt}
            </Text>

            <Divider my="sm" />

            <Text size="sm" fw={500} mb={4}>
              Modelin tespit ettiği başlıca risk faktörleri:
            </Text>

            <List size="sm" spacing={2}>
              {riskDetails.factors.map((f, idx) => (
                <List.Item key={idx}>{f}</List.Item>
              ))}
            </List>
          </Paper>
        )}

        {/* FORM – Vendor oluştur / güncelle */}
        <Paper withBorder radius="md" p="md">
          <Group justify="space-between" align="center" mb="sm">
            <Title order={4}>
              {isEdit ? "Vendor Güncelle" : "Yeni Vendor Oluştur"}
            </Title>
            {isEdit && (
              <Text size="xs" c="dimmed">
                Düzenlenen: <strong>{form.name}</strong>
              </Text>
            )}
          </Group>

          <Stack gap="sm">
            <Flex
              gap="sm"
              direction={{ base: "column", md: "row" }}
              align={{ base: "stretch", md: "flex-end" }}
            >
              <TextInput
                label="Vendor Adı"
                placeholder="Örn: TechPlus Solutions"
                value={form.name}
                onChange={(e) => {
                  const value = e.currentTarget.value;
                  setForm((prev) => ({
                    ...prev,
                    name: value,
                  }));
                }}
                w={{ base: "100%", md: "40%" }}
              />

              <NumberInput
                label="Finansal Sağlık (0-100)"
                value={form.financialHealth}
                min={0}
                max={100}
                onChange={(val) =>
                  setForm((prev) => ({
                    ...prev,
                    financialHealth: typeof val === "number" ? val : "",
                  }))
                }
              />

              <NumberInput
                label="SLA Uptime (%)"
                value={form.slaUptime}
                min={0}
                max={100}
                onChange={(val) =>
                  setForm((prev) => ({
                    ...prev,
                    slaUptime: typeof val === "number" ? val : "",
                  }))
                }
              />

              <NumberInput
                label="Son 12 Ay Major Incident"
                value={form.majorIncidents}
                min={0}
                onChange={(val) =>
                  setForm((prev) => ({
                    ...prev,
                    majorIncidents: typeof val === "number" ? val : "",
                  }))
                }
              />
            </Flex>

            {/* sadece CREATE modunda: Security Certs + Dokümanlar */}
            {!isEdit && (
              <>
                <Flex
                  gap="sm"
                  direction={{ base: "column", md: "row" }}
                  align={{ base: "stretch", md: "flex-start" }}
                  mt="sm"
                >
                  <MultiSelect
                    label="Security Certs"
                    placeholder="Seçiniz"
                    value={form.securityCerts}
                    onChange={(val) =>
                      setForm((prev) => ({
                        ...prev,
                        securityCerts: val,
                      }))
                    }
                    data={[
                      "ISO27001",
                      "SOC2",
                      "ISO27017",
                      "ISO27018",
                      "PCI-DSS",
                    ]}
                    searchable
                    clearable
                    w={{ base: "100%", md: "40%" }}
                  />

                  <Stack gap={4}>
                    <Text size="sm" fw={500}>
                      Dokümanlar
                    </Text>
                    <Checkbox
                      label="Geçerli Sözleşme"
                      checked={form.contractValid}
                      onChange={(e) => {
                        const checked = e.currentTarget.checked;
                        setForm((prev) => ({
                          ...prev,
                          contractValid: checked,
                        }));
                      }}
                    />
                    <Checkbox
                      label="Geçerli Privacy Policy"
                      checked={form.privacyPolicyValid}
                      onChange={(e) => {
                        const checked = e.currentTarget.checked;
                        setForm((prev) => ({
                          ...prev,
                          privacyPolicyValid: checked,
                        }));
                      }}
                    />
                    <Checkbox
                      label="Geçerli Pentest Raporu"
                      checked={form.pentestReportValid}
                      onChange={(e) => {
                        const checked = e.currentTarget.checked;
                        setForm((prev) => ({
                          ...prev,
                          pentestReportValid: checked,
                        }));
                      }}
                    />
                  </Stack>
                </Flex>
              </>
            )}

            <Group justify="flex-end" mt="sm">
              <Button variant="default" onClick={resetForm}>
                Temizle
              </Button>
              <Button onClick={handleSubmit}>
                {isEdit ? "Güncelle" : "Oluştur"}
              </Button>
            </Group>

            {error && (
              <Text c="red" size="sm">
                {error}
              </Text>
            )}
            {infoMessage && (
              <Text c="green" size="sm">
                {infoMessage}
              </Text>
            )}
          </Stack>
        </Paper>

        {/* FILTER BAR */}
        <Paper withBorder radius="md" p="md">
          <Flex
            justify="space-between"
            align="center"
            direction={{ base: "column", sm: "row" }}
            gap="sm"
          >
            <Group w={{ base: "100%", sm: "auto" }}>
              <TextInput
                placeholder="Vendor ara..."
                leftSection={<IconSearch size={16} />}
                value={query}
                onChange={(e) => {
                  const value = e.currentTarget.value;
                  setQuery(value);
                }}
                w={{ base: "100%", sm: 260 }}
              />
            </Group>

            <Button
              variant="default"
              leftSection={<IconRefresh size={16} />}
              onClick={() => loadData(pageIndex, pageSize, debouncedQuery)}
            >
              Yenile
            </Button>
          </Flex>
        </Paper>

        {/* TABLE + FOOTER */}
        <Paper withBorder radius="md" p={0} style={{ overflow: "hidden" }}>
          {loading ? (
            <Center p="xl">
              <Loader />
            </Center>
          ) : error && vendors.length === 0 ? (
            <Center p="xl">
              <Text c="red">{error}</Text>
            </Center>
          ) : vendors.length === 0 ? (
            <Center p="xl">
              <Text>Kayıt bulunamadı.</Text>
            </Center>
          ) : (
            <>
              <Table striped highlightOnHover verticalSpacing="sm">
                <Table.Thead>
                  <Table.Tr>
                    <Table.Th>Vendor</Table.Th>
                    <Table.Th>Finansal Sağlık</Table.Th>
                    <Table.Th>SLA Uptime</Table.Th>
                    <Table.Th>Major Incident (12 Ay)</Table.Th>
                    <Table.Th>Risk</Table.Th>
                    <Table.Th style={{ width: 220, textAlign: "right" }}>
                      Aksiyonlar
                    </Table.Th>
                  </Table.Tr>
                </Table.Thead>
                <Table.Tbody>{rows}</Table.Tbody>
              </Table>

              <Divider />

              {/* FOOTER: pagination + page size */}
              <Flex
                align="center"
                justify="space-between"
                direction="row"
                wrap="nowrap"
                p="md"
                gap="lg"
              >
                {/* Sol alan */}
                <Text size="sm" c="dimmed">
                  Sayfa {pageIndex} / {totalPages}
                </Text>

                {/* Orta alan: Pagination */}
                <Flex align="center" justify="center" style={{ flexGrow: 1 }}>
                  <Pagination
                    value={pageIndex}
                    onChange={handleChangePage}
                    total={totalPages}
                    radius="md"
                  />
                </Flex>

                {/* Sağ alan: NumberInput */}
                <NumberInput
                  size="xs"
                  description="Sayfa boyutu"
                  value={pageSize}
                  min={5}
                  max={100}
                  step={5}
                  w={120}
                  onChange={handleChangePageSize}
                />
              </Flex>
            </>
          )}
        </Paper>
      </Stack>
    </Container>
  );
};

export default VendorList;
