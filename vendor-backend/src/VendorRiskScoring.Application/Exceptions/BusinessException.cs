namespace VendorRiskScoring.Application.Exceptions;

public class BusinessException(
    string message,
    int statusCode = StatusCodes.Status400BadRequest,
    Exception? inner = null)
    : Exception(message, inner)
{
    public int StatusCode { get; } = statusCode;
}