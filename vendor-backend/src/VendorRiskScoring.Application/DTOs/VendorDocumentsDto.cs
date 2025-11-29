namespace VendorRiskScoring.Application.DTOs;

public class VendorDocumentsDto
{
    public bool ContractValid { get; init; }
    public bool PrivacyPolicyValid { get; init; }
    public bool PentestReportValid { get; init; }
}