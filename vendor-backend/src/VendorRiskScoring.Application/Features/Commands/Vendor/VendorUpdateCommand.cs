namespace VendorRiskScoring.Application.Features.Commands.Vendor;

/// <summary>Vendor bilgilerini güncellemek.</summary>
public record VendorUpdateCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int FinancialHealth { get; init; }
    public double SlaUptime { get; init; }
    public int MajorIncidents { get; init; }
    public List<string> SecurityCerts { get; init; } = [];
    public VendorDocumentsDto Documents { get; init; } = new();
}