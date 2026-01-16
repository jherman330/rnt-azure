namespace SimpleTodo.Api.Middleware;

/// <summary>
/// Middleware to generate and attach correlation IDs to all HTTP requests and responses.
/// Correlation IDs are used for request tracing and debugging across distributed systems.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private const string CorrelationIdItemKey = "CorrelationId";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Try to get correlation ID from request header, otherwise generate a new one
        string correlationId;
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var headerValue) &&
            !string.IsNullOrWhiteSpace(headerValue))
        {
            correlationId = headerValue.ToString();
        }
        else
        {
            correlationId = Guid.NewGuid().ToString();
        }

        // Store correlation ID in HttpContext.Items for access throughout the request pipeline
        context.Items[CorrelationIdItemKey] = correlationId;

        // Add correlation ID to response headers
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        // Continue processing the request
        await _next(context);
    }

    /// <summary>
    /// Extension method to retrieve the correlation ID from HttpContext.
    /// </summary>
    public static string GetCorrelationId(HttpContext context)
    {
        if (context.Items.TryGetValue(CorrelationIdItemKey, out var value) && value is string correlationId)
        {
            return correlationId;
        }
        return string.Empty;
    }
}
