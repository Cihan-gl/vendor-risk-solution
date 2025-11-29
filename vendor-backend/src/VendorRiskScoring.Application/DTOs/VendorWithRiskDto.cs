namespace VendorRiskScoring.Application.DTOs;

/// <summary>
/// Vendor bilgisini ve en güncel risk skorunu temsil eder.
/// </summary>
public class VendorWithRiskDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Finansal sağlık skoru (0-100) – financialHealth.</summary>
    public int FinancialHealth { get; set; }

    /// <summary>SLA yüzdesi – slaUptime.</summary>
    public double SlaUptime { get; set; }

    /// <summary>Son 12 ayda yaşanan major incident sayısı – majorIncidents.</summary>
    public int MajorIncidents { get; set; }

    /// <summary>En son risk değerlendirmesi.</summary>
    public RiskAssessmentDto? LatestRisk { get; set; }
}
