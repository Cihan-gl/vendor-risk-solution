namespace VendorRiskScoring.Domain.Entities;

/// <summary>
/// documents alanını temsil eder.
/// </summary>
public class VendorDocuments
{
    /// <summary>Geçerli bir sözleşme mevcut mu? – contractValid.</summary>
    public bool ContractValid { get; set; }

    /// <summary>Güncel gizlilik politikası mevcut mu? – privacyPolicyValid.</summary>
    public bool PrivacyPolicyValid { get; set; }

    /// <summary>Güncel pentest raporu mevcut mu? – pentestReportValid.</summary>
    public bool PentestReportValid { get; set; }
}