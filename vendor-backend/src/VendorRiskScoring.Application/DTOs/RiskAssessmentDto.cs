namespace VendorRiskScoring.Application.DTOs;

/// <summary>
/// Vendor’a ait bir risk değerlendirmesinin DTO versiyonudur.
/// </summary>
public class RiskAssessmentDto
{
    [JsonPropertyName("riskScore")]
    public double RiskScore { get; set; }

    [JsonPropertyName("riskLevel")]
    public string RiskLevel { get; set; } = string.Empty;

    [JsonPropertyName("reason")]
    public string Reason => string.Join(" + ", Reasons);

    [JsonIgnore]
    public List<string> Reasons { get; set; } = [];

    public DateTime CreatedAt { get; set; }
}