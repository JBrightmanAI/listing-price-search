namespace ListingSearch.Api.Infrastructure;

/// <summary>
/// Adds standard browser-hardening headers to every response. Applied in OnStarting because
/// the exception handler clears headers when it rewrites an error response.
/// </summary>
internal sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    // Scripts are locked to our own origin. Inline styles are allowed because the UI uses MUI,
    // whose styling engine (Emotion) injects <style> tags at runtime; style injection is far lower risk than script.
    private const string ContentSecurityPolicy =
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; " +
        "object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'";

    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers.XContentTypeOptions = "nosniff";
            headers.XFrameOptions = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers.ContentSecurityPolicy = ContentSecurityPolicy;
            return Task.CompletedTask;
        });

        return next(context);
    }
}
