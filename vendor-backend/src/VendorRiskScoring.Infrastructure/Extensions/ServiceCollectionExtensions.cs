namespace VendorRiskScoring.Infrastructure.Extensions;

/// <summary>
/// DI container için infrastructure bağımlılıklarını kaydeder.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // PostgreSQL bağlantısı
        var dbConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(dbConnectionString))
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection tanımlı değil.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(dbConnectionString));

        // Redis cache
        var redisConnectionString = configuration.GetConnectionString("RedisConnection");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "VendorRisk_";
            });
        }

        // DI
        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<IRiskCacheService, RedisRiskCacheService>();

        return services;
    }
}