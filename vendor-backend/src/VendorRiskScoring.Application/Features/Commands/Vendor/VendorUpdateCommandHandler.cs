namespace VendorRiskScoring.Application.Features.Commands.Vendor;

public class VendorUpdateCommandHandler(
    IVendorRepository vendorRepository,
    IRuleEngineService ruleEngineService,
    IRiskCacheService riskCacheService) : IRequestHandler<VendorUpdateCommand, Result>
{
    public async Task<Result> Handle(VendorUpdateCommand request, CancellationToken cancellationToken)
    {
        var vendor = await vendorRepository.GetByIdAsync(request.Id);
        if (vendor is null)
            return Result.Failure("Vendor bulunamadı.", StatusCodes.Status404NotFound);

        if (!string.Equals(vendor.Name, request.Name, StringComparison.OrdinalIgnoreCase))
        {
            if (await vendorRepository.ExistsByNameAsync(request.Name, cancellationToken))
            {
                return Result.Failure($"'{request.Name}' adlı vendor zaten mevcut.", StatusCodes.Status400BadRequest);
            }

            vendor.Name = request.Name;
        }

        vendor.Name = request.Name;
        vendor.FinancialHealth = request.FinancialHealth;
        vendor.SlaUptime = request.SlaUptime;
        vendor.MajorIncidents = request.MajorIncidents;
        vendor.SecurityCerts = request.SecurityCerts;

        vendor.Documents = new VendorDocuments
        {
            ContractValid = request.Documents.ContractValid,
            PrivacyPolicyValid = request.Documents.PrivacyPolicyValid,
            PentestReportValid = request.Documents.PentestReportValid
        };

        // Yeni risk skorunu hesapla
        var risk = await ruleEngineService.CalculateRiskAsync(vendor);

        var riskAssessment = new RiskAssessment
        {
            RiskScore = risk.Score,
            RiskLevel = risk.Level,
            Reasons = risk.Reasons
        };

        vendor.RiskAssessments.Add(riskAssessment);
        await vendorRepository.SaveChangesAsync();

        // Cache invalidate
        await riskCacheService.RemoveVendorRiskAsync(request.Id, cancellationToken);

        return Result.Success(StatusCodes.Status204NoContent);
    }
}