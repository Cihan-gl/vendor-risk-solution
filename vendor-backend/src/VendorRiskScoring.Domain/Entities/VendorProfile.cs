namespace VendorRiskScoring.Domain.Entities;

/// <summary>
/// Tedarikçi (vendor) bilgilerini ve güvenlik, operasyonel, finansal durumunu temsil eder.
/// </summary>
public class VendorProfile : BaseEntity
{
    /// <summary>Vendor adı.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Finansal sağlık puanı (0-100) – financialHealth.</summary>
    public int FinancialHealth { get; set; }

    /// <summary>Servis kullanılabilirliği yüzdesi (örn: %99.9) – slaUptime.</summary>
    public double SlaUptime { get; set; }

    /// <summary>Yıl içinde yaşanan büyük olay (incident) sayısı – majorIncidents.</summary>
    public int MajorIncidents { get; set; }

    /// <summary>Vendor'ın sahip olduğu güvenlik sertifikaları – securityCerts.</summary>
    public List<string> SecurityCerts { get; set; } = [];

    /// <summary>Vendor'ın sözleşme, gizlilik politikası ve pentest doküman durumlarını tutar – documents.</summary>
    public VendorDocuments Documents { get; set; } = new();

    /// <summary>Vendor’a ait geçmiş risk skorları.</summary>
    public List<RiskAssessment> RiskAssessments { get; set; } = [];
}