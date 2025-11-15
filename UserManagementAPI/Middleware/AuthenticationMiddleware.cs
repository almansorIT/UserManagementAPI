using System.Text.Json;

namespace UserManagementAPI.Middleware;

/// <summary>
/// Middleware for token-based authentication.
/// Validates tokens from incoming requests and allows access only to users with valid tokens.
/// Returns a 401 Unauthorized response for invalid or missing tokens.
/// This middleware should be registered after error handling but before logging.
/// </summary>
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;

    // In production, this should be loaded from configuration/secrets
    // In production, this should be loaded from configuration/secrets
private const string ValidToken = "secret-api-token-12345"; // REMOVE "Bearer "
    
    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Allow public endpoints without authentication (optional)
        if (IsPublicEndpoint(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var token = ExtractToken(context.Request);

        if (string.IsNullOrEmpty(token) || !ValidateToken(token))
        {
            _logger.LogWarning("Unauthorized access attempt at {Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var response = new UnauthorizedResponse
            {
                Error = "Unauthorized",
                Message = "Invalid or missing authentication token."
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
            return;
        }

        _logger.LogInformation("Authorized request at {Path}", context.Request.Path);
        await _next(context);
    }

    /// <summary>
    /// Extracts the Bearer token from the Authorization header.
    /// </summary>
    private string? ExtractToken(HttpRequest request)
    {
        var authHeader = request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(authHeader))
            return null;

        if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return authHeader["Bearer ".Length..].Trim();

        return null;
    }

    /// <summary>
    /// Validates the token.
    /// In production, this should validate against a database, JWT signature, etc.
    /// </summary>
    private bool ValidateToken(string token)
    {
        return token.Equals(ValidToken, StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines if an endpoint is public and doesn't require authentication.
    /// </summary>
    private static bool IsPublicEndpoint(PathString path)
    {
        // Define public endpoints here (if any)
        // For example: health checks, login endpoints, etc.
        return false; // All endpoints require authentication for this API
    }
}

/// <summary>
/// Response model for unauthorized requests.
/// </summary>
public class UnauthorizedResponse
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Extension method to register the authentication middleware.
/// </summary>
public static class AuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseTokenAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthenticationMiddleware>();
    }
}

/// <summary>
/// Extension method to write response asynchronously.
/// </summary>
public static class HttpResponseExtensions
{
    public static async Task WriteAsyncSafe(this HttpResponse response, string content)
    {
        try
        {
            await response.WriteAsync(content);
        }
        catch (Exception ex)
        {
            // Handle case where client disconnected, etc.
            System.Diagnostics.Debug.WriteLine($"Error writing response: {ex.Message}");
        }
    }
}
