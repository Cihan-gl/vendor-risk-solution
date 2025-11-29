namespace VendorRiskScoring.Application.Configurations;

/// <summary>
/// Sezonluk risk ayarları
/// </summary>
public class SeasonalRiskConfig
{
    /// <summary>Hangi aylar “yüksek sezon” kabul ediliyor (1-12).</summary>
    public List<int> HighSeasonMonths { get; set; } = [11, 12];

    /// <summary>İsmi bu anahtar kelimeleri içeren vendor'lara seasonal risk uygulanır.</summary>
    public List<string> LogisticsKeywords { get; set; } = ["Logistics", "Freight", "Transport"];

    /// <summary>Yüksek sezonda ekstra risk artışı (0-1 arası normalize skor üzerine eklenir).</summary>
    public double ExtraRisk { get; set; } = 0.05;
}