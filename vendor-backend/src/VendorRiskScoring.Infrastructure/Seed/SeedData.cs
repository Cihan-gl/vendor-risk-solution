namespace VendorRiskScoring.Infrastructure.Seed;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();

        if (await context.Vendors.AnyAsync())
            return;

        // JSON PATH (runtime base directory)
        var filePath = Path.Combine(AppContext.BaseDirectory, "SampleVendorData.json");

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Seed file not found: {filePath}");

        var json = await File.ReadAllTextAsync(filePath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var data = JsonSerializer.Deserialize<VendorSeedModel>(json, options);

        if (data is null || data.Vendors.Count == 0)
            return;

        var vendorEntities = new List<VendorProfile>();

        foreach (var item in data.Vendors)
        {
            var vendor = new VendorProfile
            {
                Name = item.Name,
                FinancialHealth = item.FinancialHealth,
                SlaUptime = item.SlaUptime,
                MajorIncidents = item.MajorIncidents,
                SecurityCerts = item.SecurityCerts,
                Documents = new VendorDocuments
                {
                    ContractValid = item.Documents.ContractValid,
                    PrivacyPolicyValid = item.Documents.PrivacyPolicyValid,
                    PentestReportValid = item.Documents.PentestReportValid
                }
            };

            // İlk risk kaydı placeholder (ilk GET çağrısında rule engine hesaplayacak)
            vendor.RiskAssessments.Add(new RiskAssessment
            {
                RiskScore = 0,
                RiskLevel = "Unknown",
                Reasons = ["Initial seed — will be recalculated"]
            });

            vendorEntities.Add(vendor);
        }

        await context.Vendors.AddRangeAsync(vendorEntities);
        await context.SaveChangesAsync();
    }
}