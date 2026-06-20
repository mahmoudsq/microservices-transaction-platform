using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Correlation;

public class CorrelationMiddleware(RequestDelegate next)
{
    private const string HeaderName = "X-Correlation-ID";
    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext context)
    {
        var correlationId = context.Request.Headers[HeaderName].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items[HeaderName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        await _next(context);
    }
}
