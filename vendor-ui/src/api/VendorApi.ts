import axios from "axios";
import type {
  Result,
  PaginatedList,
  VendorWithRiskDto,
  RiskAssessmentDto,
} from "../types/vendor";

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL || "http://localhost:5207/api";

const api = axios.create({
  baseURL: API_BASE_URL,
});

export interface GetVendorListParams {
  pageIndex: number;
  pageSize: number;
  sort?: string;
  query?: string;
}

export interface VendorDocumentsPayload {
  contractValid: boolean;
  privacyPolicyValid: boolean;
  pentestReportValid: boolean;
}

export interface VendorCreateUpdatePayload {
  name: string;
  financialHealth: number;
  slaUptime: number;
  majorIncidents: number;
  securityCerts: string[];
  documents: VendorDocumentsPayload;
}

export const VendorApi = {
  getList(params: GetVendorListParams) {
    return api.get<Result<PaginatedList<VendorWithRiskDto>>>(
      "/v1/Vendor/list-with-pagination",
      { params }
    );
  },

  createVendor(payload: VendorCreateUpdatePayload) {
    return api.post<Result<string>>("/v1/Vendor", payload);
  },

  updateVendor(
    id: string,
    payload: VendorCreateUpdatePayload & { id: string }
  ) {
    return api.put<void>(`/v1/Vendor/${id}`, payload);
  },

  deleteVendor(id: string) {
    return api.delete<void>(`/v1/Vendor/${id}`);
  },

  getRisk(id: string) {
    return api.get<Result<RiskAssessmentDto>>(`/v1/Vendor/${id}/risk`);
  },
};
