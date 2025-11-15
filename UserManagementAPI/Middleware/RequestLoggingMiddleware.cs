namespace UserManagementAPI.Middleware;

/// <summary>
/// Middleware for logging all incoming requests and outgoing responses for auditing purposes.
/// Logs HTTP method, request path, and response status code.
/// This middleware should be registered last in the pipeline.
/// </summary>
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
        var method = context.Request.Method;
        var path = context.Request.Path;
        var queryString = context.Request.QueryString;

        _logger.LogInformation("Incoming Request: {Method} {Path}{QueryString}", method, path, queryString);

        await _next(context);

        var statusCode = context.Response.StatusCode;
        _logger.LogInformation("Outgoing Response: {Method} {Path} => {StatusCode}", method, path, statusCode);
    }
}

/// <summary>
/// Extension method to register the request logging middleware.
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
