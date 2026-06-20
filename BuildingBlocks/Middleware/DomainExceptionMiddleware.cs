using System.Text.Json;
using BuildingBlocks.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;

namespace BuildingBlocks.Middleware;

public class DomainExceptionMiddleware(RequestDelegate next, IStringLocalizerFactory localizerFactory)
{
    private readonly IStringLocalizer _localizer = localizerFactory.Create(typeof(DomainExceptionMiddleware));

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                title = "Domain Rule Violation",
                status = 400,
                detail = _localizer[ex.Code].Value,
                code = ex.Code
            }));
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            context.Response.ContentType = "application/problem+json";

            var translatedErrors = ex.Errors.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Select(code => _localizer[code].Value).ToArray());

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.20",
                title = "Validation Failed",
                status = 422,
                errors = translatedErrors
            }));
        }
    }
}
