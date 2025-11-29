namespace VendorRiskScoring.Application.Extensions;

/// <summary>
/// DI container için Application bağımlılıklarını kaydeder.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IRuleEngineService, RuleEngineService>();
        return services;
    }
}