namespace VendorRiskScoring.Application.Features.Queries.Vendor;

public class GetVendorRiskQueryHandler(
    IVendorRepository vendorRepository,
    IRuleEngineService ruleEngine,
    IRiskCacheService cacheService
) : IRequestHandler<GetVendorRiskQuery, Result<RiskAssessmentDto>>
{
    public async Task<Result<RiskAssessmentDto>> Handle(GetVendorRiskQuery request, CancellationToken cancellationToken)
    {
        // 1) Cache kontrol
        var cached = await cacheService.GetVendorRiskAsync(request.VendorId, cancellationToken);
        if (cached != null)
        {
            return Result<RiskAssessmentDto>.Success(cached);
        }

        // 2) DB'den vendor
        var vendor = await vendorRepository.GetByIdAsync(request.VendorId);
        if (vendor is null)
            return Result<RiskAssessmentDto>.Failure("Vendor bulunamadı.", StatusCodes.Status404NotFound);

        // 3) Eğer mevcut risk varsa kullan
        var latest = vendor.RiskAssessments
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefault();

        if (latest != null && latest.RiskLevel != "Unknown")
        {
            var dto = new RiskAssessmentDto
            {
                RiskScore = latest.RiskScore,
                RiskLevel = latest.RiskLevel,
                CreatedAt = latest.CreatedAt,
                Reasons = latest.Reasons
            };

            // Cache'e yaz
            await cacheService.SetVendorRiskAsync(request.VendorId, dto, cancellationToken);

            return Result<RiskAssessmentDto>.Success(dto);
        }

        // 4) Risk yoksa rule engine çalıştır
        var risk = await ruleEngine.CalculateRiskAsync(vendor);

        var newEntity = new RiskAssessment
        {
            VendorId = vendor.Id,
            RiskScore = risk.Score,
            RiskLevel = risk.Level,
            Reasons = risk.Reasons
        };

        vendor.RiskAssessments.Add(newEntity);
        await vendorRepository.SaveChangesAsync();

        var newDto = new RiskAssessmentDto
        {
            RiskScore = newEntity.RiskScore,
            RiskLevel = newEntity.RiskLevel,
            CreatedAt = newEntity.CreatedAt,
            Reasons = newEntity.Reasons
        };

        await cacheService.SetVendorRiskAsync(request.VendorId, newDto, cancellationToken);

        return Result<RiskAssessmentDto>.Success(newDto);
    }
}