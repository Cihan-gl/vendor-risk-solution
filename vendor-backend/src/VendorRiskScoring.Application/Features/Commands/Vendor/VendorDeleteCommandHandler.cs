namespace VendorRiskScoring.Application.Features.Commands.Vendor;

public class VendorDeleteCommandHandler(IVendorRepository vendorRepository, IRiskCacheService riskCacheService)
    : IRequestHandler<VendorDeleteCommand, Result>
{
    public async Task<Result> Handle(VendorDeleteCommand request, CancellationToken cancellationToken)
    {
        var vendor = await vendorRepository.GetByIdAsync(request.Id);
        if (vendor is null)
            return Result.Failure("Vendor bulunamadı.", StatusCodes.Status404NotFound);

        await vendorRepository.DeleteAsync(vendor);
        await vendorRepository.SaveChangesAsync();

        // Cache temizle
        await riskCacheService.RemoveVendorRiskAsync(request.Id, cancellationToken);

        return Result.Success(StatusCodes.Status204NoContent);
    }
}