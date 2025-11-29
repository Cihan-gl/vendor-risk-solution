namespace VendorRiskScoring.Infrastructure.Seed.Models;

public class VendorSeedModel
{
    [JsonPropertyName("Vendors")] public List<VendorSeedItem> Vendors { get; set; } = new();
}

public class VendorSeedItem
{
    [JsonPropertyName("id")] public int Id { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("financialHealth")] public int FinancialHealth { get; set; }

    [JsonPropertyName("slaUptime")] public double SlaUptime { get; set; }

    [JsonPropertyName("majorIncidents")] public int MajorIncidents { get; set; }

    [JsonPropertyName("securityCerts")] public List<string> SecurityCerts { get; set; } = new();

    [JsonPropertyName("documents")] public VendorSeedDocument Documents { get; set; } = new();
}

public class VendorSeedDocument
{
    [JsonPropertyName("contractValid")] public bool ContractValid { get; set; }

    [JsonPropertyName("privacyPolicyValid")]
    public bool PrivacyPolicyValid { get; set; }

    [JsonPropertyName("pentestReportValid")]
    public bool PentestReportValid { get; set; }
}