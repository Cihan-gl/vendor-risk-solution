namespace VendorRiskScoring.Application.Features.Commands.Vendor;

/// <summary>Belirli bir vendor’ı silmek </summary>
public record VendorDeleteCommand(Guid Id) : IRequest<Result>;