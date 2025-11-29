namespace VendorRiskScoring.Application.Features.Commands.Vendor;

public class VendorCreateCommandHandler(IRuleEngineService ruleEngineService, IVendorRepository vendorRepo)
    : IRequestHandler<VendorCreateCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(VendorCreateCommand request, CancellationToken cancellationToken)
    {
        if (await vendorRepo.ExistsByNameAsync(request.Name, cancellationToken))
        {
            return Result<Guid>.Failure($"'{request.Name}' adlı vendor zaten mevcut.", StatusCodes.Status400BadRequest);
        }

        var vendor = new VendorProfile
        {
            Name = request.Name,
            FinancialHealth = request.FinancialHealth,
            SlaUptime = request.SlaUptime,
            MajorIncidents = request.MajorIncidents,
            SecurityCerts = request.SecurityCerts,
            Documents = new VendorDocuments
            {
                ContractValid = request.Documents.ContractValid,
                PrivacyPolicyValid = request.Documents.PrivacyPolicyValid,
                PentestReportValid = request.Documents.PentestReportValid
            }
        };

        var risk = await ruleEngineService.CalculateRiskAsync(vendor);

        var riskAssessment = new RiskAssessment
        {
            RiskScore = risk.Score,
            RiskLevel = risk.Level,
            Reasons = risk.Reasons
        };

        vendor.RiskAssessments.Add(riskAssessment);

        await vendorRepo.AddAsync(vendor);
        await vendorRepo.SaveChangesAsync();

        return Result<Guid>.Success(vendor.Id, StatusCodes.Status201Created);
    }
}