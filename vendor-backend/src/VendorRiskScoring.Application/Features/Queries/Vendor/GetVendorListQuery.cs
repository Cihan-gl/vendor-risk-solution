namespace VendorRiskScoring.Application.Features.Queries.Vendor;

/// <summary>Tüm vendor listesini getirir (en son risk skoru ile).</summary>
public class GetVendorListQuery : IRequest<Result<PaginatedList<VendorWithRiskDto>>>
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Sort { get; set; }
    public string? Query { get; set; }
}