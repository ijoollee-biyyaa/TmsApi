using System.Diagnostics;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = Guid.NewGuid().ToString("N")[..8];

        context.Response.Headers["X-Correlation-ID"] = correlationId;
        var stopwatch = Stopwatch.StartNew();

        // Log the incoming request
        _logger.LogInformation("Request Started: {Method} {Path} (Correlation ID: {CorrelationId})", context.Request.Method, context.Request.Path, correlationId);
        // Call the next middleware in the pipeline
        await _next(context);
stopwatch.Stop();
        // Log the outgoing response
        _logger.LogInformation("Request Completed: {StatusCode} Duration: {Duration}ms (Correlation ID: {CorrelationId})", context.Response.StatusCode, stopwatch.ElapsedMilliseconds, correlationId);
    }
}