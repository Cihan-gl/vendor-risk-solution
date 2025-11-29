namespace VendorRiskScoring.Application.Exceptions;

public class ValidationException : Exception
{
    public List<string> Errors { get; }

    public ValidationException(IEnumerable<FluentValidation.Results.ValidationFailure> failures) : base(
        "Validation failed.")
    {
        Errors = failures.Select(f => f.ErrorMessage).ToList();
    }
}