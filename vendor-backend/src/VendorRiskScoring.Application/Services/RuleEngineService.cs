namespace VendorRiskScoring.Application.Services;

/// <summary>
/// Rule-based Vendor Risk Engine:
/// - RiskFactorMatrix.json içindeki similarity değerlerini matematiksel olarak kullanır
/// - Financial / Operational / Security+Compliance skorlarını 0–1 arası hesaplar
/// - FinalScore = 0.4 * Financial + 0.3 * Operational + 0.3 * SecurityCompliance
/// - Seasonal risk adjustment uygular (lojistik vendor + high season aylar)
/// - Açıklanabilir (explainable) reasons üretir.
/// </summary>
public class RuleEngineService(IOptions<RiskFactorMatrix> options) : IRuleEngineService
{
    private readonly RiskFactorMatrix _matrix = options.Value;

    public Task<RiskResultDto> CalculateRiskAsync(VendorProfile vendor)
    {
        var reasons = new List<string>();

        var financialRisk = ComputeFinancialRisk(vendor, reasons);
        var operationalRisk = ComputeOperationalRisk(vendor, reasons);
        var securityRisk = ComputeSecurityComplianceRisk(vendor, reasons);

        var finalScore = financialRisk * 0.4
                         + operationalRisk * 0.3
                         + securityRisk * 0.3;

        // 🎯 Seasonal katman
        finalScore = ApplySeasonalAdjustment(vendor, finalScore, reasons);

        finalScore = Math.Clamp(finalScore, 0, 1);

        var level = finalScore switch
        {
            <= 0.25 => "Low",
            <= 0.50 => "Medium",
            <= 0.75 => "High",
            _ => "Critical"
        };

        return Task.FromResult(new RiskResultDto
        {
            Score = Math.Round(finalScore, 2),
            Level = level,
            Reasons = reasons
        });
    }

    // -------------------------------------------------------
    //  FINANCIAL
    // -------------------------------------------------------
    private double ComputeFinancialRisk(VendorProfile vendor, List<string> reasons)
    {
        var triggered = new List<(string Key, Dictionary<string, double> Similar)>();

        // FinancialHealth < 50 → lowCashFlow + highDebtRatio
        // 50–80 arası → creditDowngrade
        // >80 → finansal risk yok (0)
        if (vendor.FinancialHealth < 50 && _matrix.FinancialRisk.Count > 0)
        {
            AddIfExists(_matrix.FinancialRisk, "lowCashFlow", triggered);
            AddIfExists(_matrix.FinancialRisk, "highDebtRatio", triggered);

            reasons.Add($"Financial health {vendor.FinancialHealth} < 50: low cash flow / high debt risk.");
        }
        else if (vendor.FinancialHealth is >= 50 and < 80)
        {
            AddIfExists(_matrix.FinancialRisk, "creditDowngrade", triggered);
            reasons.Add($"Financial health {vendor.FinancialHealth}: moderate credit downgrade risk.");
        }
        else
        {
            // 80+ için finansal risk yok sayıyoruz → skor = 0
        }

        return ComputeDimensionScore(triggered, "financial", reasons);
    }

    // -------------------------------------------------------
    //  OPERATIONAL
    // -------------------------------------------------------
    private double ComputeOperationalRisk(VendorProfile vendor, List<string> reasons)
    {
        var triggered = new List<(string Key, Dictionary<string, double> Similar)>();

        if (vendor.SlaUptime < 95 && _matrix.OperationalRisk.Count > 0)
        {
            AddIfExists(_matrix.OperationalRisk, "slaDrop", triggered);
            reasons.Add($"SLA {vendor.SlaUptime}% < 95%: SLA drop risk.");
        }

        if (vendor.MajorIncidents > 0 && _matrix.OperationalRisk.Count > 0)
        {
            AddIfExists(_matrix.OperationalRisk, "majorIncident", triggered);
            reasons.Add($"Last 12 months major incidents: {vendor.MajorIncidents}.");
        }

        return ComputeDimensionScore(triggered, "operational", reasons);
    }

    // -------------------------------------------------------
    //  SECURITY + COMPLIANCE
    // -------------------------------------------------------
    private double ComputeSecurityComplianceRisk(VendorProfile vendor, List<string> reasons)
    {
        var triggered = new List<(string Key, Dictionary<string, double> Similar)>();

        // SECURITY: missing ISO, failed pentest
        var hasIso27001 = vendor.SecurityCerts.Any(c =>
            c.Equals("ISO27001", StringComparison.OrdinalIgnoreCase) ||
            c.Equals("ISO 27001", StringComparison.OrdinalIgnoreCase));

        if (!hasIso27001 && _matrix.SecurityRisk.Count > 0)
        {
            AddIfExists(_matrix.SecurityRisk, "missingISO27001", triggered);
            reasons.Add("Missing ISO27001: security baseline not certified.");
        }

        if (!vendor.Documents.PentestReportValid && _matrix.SecurityRisk.Count > 0)
        {
            AddIfExists(_matrix.SecurityRisk, "failedPenTest", triggered);
            reasons.Add("No recent valid pentest: potential internal vulnerabilities.");
        }

        // COMPLIANCE: expired privacy policy, contract
        if (!vendor.Documents.PrivacyPolicyValid && _matrix.ComplianceRisk.Count > 0)
        {
            AddIfExists(_matrix.ComplianceRisk, "expiredPrivacyPolicy", triggered);
            reasons.Add("Privacy policy expired: potential GDPR / legal conflict.");
        }

        if (!vendor.Documents.ContractValid && _matrix.ComplianceRisk.Count > 0)
        {
            AddIfExists(_matrix.ComplianceRisk, "expiredContract", triggered);
            reasons.Add("Contract not valid: contractual / legal dispute risk.");
        }

        return ComputeDimensionScore(triggered, "security/compliance", reasons);
    }

    // -------------------------------------------------------
    //  SEASONAL RISK
    // -------------------------------------------------------
    private double ApplySeasonalAdjustment(
        VendorProfile vendor,
        double currentScore,
        List<string> reasons)
    {
        var seasonal = _matrix.SeasonalRisk;
        if (seasonal is null)
            return currentScore;

        // Vendor lojistik mi? (isimde Logistics / Freight / Transport vs.)
        var keywords = seasonal.LogisticsKeywords ?? new List<string>();
        var isLogisticsVendor = keywords.Any(k =>
            !string.IsNullOrWhiteSpace(k) &&
            vendor.Name.Contains(k, StringComparison.OrdinalIgnoreCase));

        if (!isLogisticsVendor)
            return currentScore;

        var month = DateTime.UtcNow.Month;
        var highSeasonMonths = seasonal.HighSeasonMonths ?? new List<int>();

        if (!highSeasonMonths.Contains(month))
            return currentScore;

        var extra = seasonal.ExtraRisk;
        var adjusted = currentScore + extra;

        reasons.Add(
            $"Seasonal adjustment: month {month} is high season for logistics vendors. +" +
            $"{extra:0.00} extra risk applied.");

        return adjusted;
    }

    // -------------------------------------------------------
    //  COMMON HELPERS
    // -------------------------------------------------------
    private static void AddIfExists(
        Dictionary<string, Dictionary<string, double>> source,
        string key,
        List<(string Key, Dictionary<string, double> Similar)> list)
    {
        if (source.TryGetValue(key, out var similar))
        {
            list.Add((key, similar));
        }
    }

    /// <summary>
    /// Her tetiklenen risk item için:
    /// itemScore = (1.0 (primary risk) + avg(similarityValues)) / 2
    /// Sonra dimensionScore = triggered itemScore ortalaması.
    /// </summary>
    private static double ComputeDimensionScore(
        List<(string Key, Dictionary<string, double> Similar)> triggered,
        string dimensionName,
        List<string> reasons)
    {
        if (triggered.Count == 0)
            return 0.0;

        var scores = new List<double>();

        foreach (var (key, similarDict) in triggered)
        {
            if (similarDict.Count == 0)
            {
                scores.Add(1.0);
                continue;
            }

            var avgSimilarity = similarDict.Values.Average(); // 0.75–0.9 arası gibi
            var itemScore = (1.0 + avgSimilarity) / 2.0; // 0.87 vs.

            scores.Add(itemScore);

            var topSimilar = similarDict
                .OrderByDescending(kv => kv.Value)
                .Take(2)
                .Select(kv => $"{kv.Key} ({kv.Value:0.00})");

            var sb = new StringBuilder();
            sb.Append($"{dimensionName} risk item '{key}' has similar patterns: ");
            sb.Append(string.Join(", ", topSimilar));

            reasons.Add(sb.ToString());
        }

        return Math.Clamp(scores.Average(), 0, 1);
    }
}