namespace VendorRiskScoring.Application.Interfaces;

public interface IRiskCacheService
{
    Task<RiskAssessmentDto?> GetVendorRiskAsync(Guid vendorId, CancellationToken cancellationToken = default);
    Task SetVendorRiskAsync(Guid vendorId, RiskAssessmentDto dto, CancellationToken cancellationToken = default);
    Task RemoveVendorRiskAsync(Guid vendorId, CancellationToken cancellationToken = default);
}