namespace VendorRiskScoring.API.Validators.Vendor;

/// <summary>
/// Vendor silme komutu için doğrulama kuralları.
/// </summary>
public class VendorDeleteCommandValidator : AbstractValidator<VendorDeleteCommand>
{
    public VendorDeleteCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Vendor ID boş olamaz.")
            .NotEqual(Guid.Empty).WithMessage("Geçerli bir Vendor ID girilmelidir.");
    }
}