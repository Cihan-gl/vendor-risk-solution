namespace VendorRiskScoring.API.Validators.Vendor;

/// <summary>
/// VendorUpdateCommand için alan doğrulamaları.
/// </summary>
public class VendorUpdateCommandValidator : AbstractValidator<VendorUpdateCommand>
{
    public VendorUpdateCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Vendor ID boş olamaz.")
            .NotEqual(Guid.Empty).WithMessage("Geçerli bir Vendor ID girilmelidir.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Vendor adı boş olamaz.")
            .MaximumLength(100);

        RuleFor(x => x.FinancialHealth)
            .InclusiveBetween(0, 100).WithMessage("Finansal sağlık skoru 0-100 arası olmalı.");

        RuleFor(x => x.SlaUptime)
            .InclusiveBetween(0, 100).WithMessage("SLA yüzdesi 0-100 arası olmalı.");

        RuleFor(x => x.MajorIncidents)
            .GreaterThanOrEqualTo(0).WithMessage("Major incident sayısı negatif olamaz.");
    }
}