namespace VendorRiskScoring.API.Middlewares;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var traceId = context.TraceIdentifier;

        try
        {
            await next(context);
        }
        catch (Application.Exceptions.ValidationException vex)
        {
            logger.LogWarning(vex, "Validation error occurred TraceId={TraceId}, Errors={Errors}", traceId, vex.Errors);
            var errorResult = Result.Failure(vex.Errors, StatusCodes.Status400BadRequest);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(errorResult);
        }
        catch (BusinessException bex)
        {
            logger.LogWarning(bex,
                "Business error occurred TraceId={TraceId}, Message={Message}, StatusCode={StatusCode}", traceId,
                bex.Message, bex.StatusCode);
            var errorResult = Result.Failure(bex.Message, bex.StatusCode);

            context.Response.StatusCode = bex.StatusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(errorResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception TraceId={TraceId}, Path={Path}, Method={Method}", traceId,
                context.Request.Path, context.Request.Method);
            var errorResult = Result.Failure("Beklenmeyen sunucu hatası", StatusCodes.Status500InternalServerError);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(errorResult);
        }
    }
}