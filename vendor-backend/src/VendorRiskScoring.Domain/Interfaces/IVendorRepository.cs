namespace VendorRiskScoring.Domain.Interfaces;

/// <summary>
/// Vendor verilerine erişimi soyutlayan repository arayüzü.
/// </summary>
public interface IVendorRepository
{
    Task<List<VendorProfile>> GetAllAsync();
    IQueryable<VendorProfile> GetAllQueryable();
    Task<VendorProfile?> GetByIdAsync(Guid id);
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(VendorProfile vendor);

    /// <summary>Vendoru siler.</summary>
    Task DeleteAsync(VendorProfile vendor);

    Task SaveChangesAsync();
}