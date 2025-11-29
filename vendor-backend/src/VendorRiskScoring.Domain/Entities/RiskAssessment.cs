namespace VendorRiskScoring.Domain.Entities;

/// <summary>
/// Vendor'a ait risk skoru ve açıklamaları (veritabanında tutulur).
/// </summary>
public class RiskAssessment : BaseEntity
{
    public Guid VendorId { get; set; }
    public VendorProfile Vendor { get; set; } = null!;

    /// <summary>0-1 arası normalleştirilmiş risk skoru – riskScore.</summary>
    public double RiskScore { get; set; }

    /// <summary>Risk seviyesi (Low / Medium / High / Critical) – riskLevel.</summary>
    public string RiskLevel { get; set; } = string.Empty;

    /// <summary>Detaylı açıklama maddeleri.</summary>
    public List<string> Reasons { get; set; } = [];
}