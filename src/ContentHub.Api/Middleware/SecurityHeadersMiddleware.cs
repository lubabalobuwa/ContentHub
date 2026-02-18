using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ContentHub.Api.Middleware
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "DENY";
            headers["Referrer-Policy"] = "no-referrer";
            headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
            headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "script-src 'self' https://challenges.cloudflare.com; " +
                "style-src 'self' 'unsafe-inline'; " +
                "img-src 'self' data: https:; " +
                "connect-src 'self' https://challenges.cloudflare.com; " +
                "frame-src https://challenges.cloudflare.com; " +
                "base-uri 'none'; frame-ancestors 'none';";

            await _next(context);
        }
    }
}
