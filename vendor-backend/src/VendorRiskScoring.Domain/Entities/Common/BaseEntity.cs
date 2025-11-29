namespace VendorRiskScoring.Domain.Entities.Common;

/// <summary>
/// Tüm Entity'ler için ortak alanları içerir.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; }

    /// <summary>Oluşturulma zamanı (UTC).</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Son güncellenme zamanı (UTC).</summary>
    public DateTime? UpdatedAt { get; set; }
}