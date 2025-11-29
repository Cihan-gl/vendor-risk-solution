namespace VendorRiskScoring.Application.Features.Queries.Vendor;

/// <summary>Tüm vendor listesini getirir (en son risk skoru ile).</summary>
public class GetVendorListQueryHandler(IVendorRepository vendorRepository)
    : IRequestHandler<GetVendorListQuery, Result<PaginatedList<VendorWithRiskDto>>>
{
    public async Task<Result<PaginatedList<VendorWithRiskDto>>> Handle(GetVendorListQuery request,
        CancellationToken cancellationToken)
    {
        var vendors = vendorRepository.GetAllQueryable();

        if (!string.IsNullOrEmpty(request.Query))
            vendors = vendors.Where(vendor => vendor.Name.ToLower().Contains(request.Query.ToLower()));

        var query = vendors.Select(v => new VendorWithRiskDto
        {
            Id = v.Id,
            Name = v.Name,
            FinancialHealth = v.FinancialHealth,
            SlaUptime = v.SlaUptime,
            MajorIncidents = v.MajorIncidents,
            LatestRisk = v.RiskAssessments
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new RiskAssessmentDto
                {
                    RiskScore = r.RiskScore,
                    RiskLevel = r.RiskLevel,
                    CreatedAt = r.CreatedAt,
                    Reasons = r.Reasons
                })
                .FirstOrDefault()
        });

        var result = await query
            .OrderBy(request.Sort ?? "Name desc")
            .PaginatedListAsync(request.PageIndex, request.PageSize, cancellationToken
            );

        return Result<PaginatedList<VendorWithRiskDto>>.Success(result);
    }
}