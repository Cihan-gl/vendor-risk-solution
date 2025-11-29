namespace VendorRiskScoring.Infrastructure.Services;

/// <summary>
/// Vendor risk skorlarını Redis üzerinde cache'ler.
/// Key formatı: vendor:risk:{vendorId}
/// </summary>
public class RedisRiskCacheService(IDistributedCache cache) : IRiskCacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private static string BuildKey(Guid vendorId) => $"vendor:risk:{vendorId}";

    public async Task<RiskAssessmentDto?> GetVendorRiskAsync(Guid vendorId,
        CancellationToken cancellationToken = default)
    {
        var key = BuildKey(vendorId);
        var json = await cache.GetStringAsync(key, cancellationToken);

        if (string.IsNullOrEmpty(json))
            return null;

        return JsonSerializer.Deserialize<RiskAssessmentDto>(json, JsonOptions);
    }

    public async Task SetVendorRiskAsync(Guid vendorId, RiskAssessmentDto dto,
        CancellationToken cancellationToken = default)
    {
        var key = BuildKey(vendorId);
        var json = JsonSerializer.Serialize(dto, JsonOptions);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // 10 dk cache
        };

        await cache.SetStringAsync(key, json, options, cancellationToken);
    }

    public async Task RemoveVendorRiskAsync(Guid vendorId, CancellationToken cancellationToken = default)
    {
        var key = BuildKey(vendorId);
        await cache.RemoveAsync(key, cancellationToken);
    }
}