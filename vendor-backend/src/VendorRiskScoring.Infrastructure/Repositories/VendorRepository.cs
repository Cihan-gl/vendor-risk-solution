namespace VendorRiskScoring.Infrastructure.Repositories;

/// <summary>
/// Vendor veri erişim işlemlerini EF Core ile gerçekleştirir.
/// </summary>
public class VendorRepository(AppDbContext context) : IVendorRepository
{
    public async Task<List<VendorProfile>> GetAllAsync()
    {
        return await context.Vendors
            .Include(v => v.RiskAssessments)
            .AsNoTracking()
            .ToListAsync();
    }

    public IQueryable<VendorProfile> GetAllQueryable()
    {
        return context.Vendors.AsNoTracking();
    }

    public async Task<VendorProfile?> GetByIdAsync(Guid id)
    {
        return await context.Vendors
            .Include(v => v.RiskAssessments)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task AddAsync(VendorProfile vendor)
    {
        await context.Vendors.AddAsync(vendor);
    }

    public Task DeleteAsync(VendorProfile vendor)
    {
        context.Vendors.Remove(vendor);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await context.Vendors
            .AnyAsync(v => v.Name.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Vendors.AnyAsync(v => v.Id == id, cancellationToken);
    }

    public async Task SaveChangesAsync()
    {
        await context.SaveChangesAsync();
    }
}