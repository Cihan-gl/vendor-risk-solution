namespace VendorRiskScoring.Application.Interfaces;

/// <summary>
/// Vendor risk skorlamasını gerçekleştiren servis.
/// </summary>
public interface IRuleEngineService
{
    /// <summary>Vendor’a ait risk skorunu hesaplar.</summary>
    Task<RiskResultDto> CalculateRiskAsync(VendorProfile vendor);
}