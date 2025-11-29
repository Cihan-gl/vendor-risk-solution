namespace VendorRiskScoring.Application.Features.Queries.Vendor;

/// <summary>Belirli bir vendor’ın güncel risk skorunu getirir.</summary>
public record GetVendorRiskQuery(Guid VendorId) : IRequest<Result<RiskAssessmentDto>>;