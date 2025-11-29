namespace VendorRiskScoring.Application.DTOs;

/// <summary>
/// Risk skoru ve açıklamasını dönen DTO.
/// </summary>
public class RiskResultDto
{
    /// <summary>0-1 arası normalize final risk skoru.</summary>
    public double Score { get; set; }

    /// <summary>Risk seviyesi (Low / Medium / High / Critical).</summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>İnsan tarafından okunabilir açıklamalar.</summary>
    public List<string> Reasons { get; set; } = new();
}