using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace ContentHub.Api.Middleware
{
    public class CorrelationIdMiddleware
    {
        private const string CorrelationIdHeader = "X-Correlation-ID";
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = GetCorrelationId(context);
            context.TraceIdentifier = correlationId;
            context.Response.Headers[CorrelationIdHeader] = correlationId;

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }

        private static string GetCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out StringValues value) &&
                !StringValues.IsNullOrEmpty(value))
            {
                return value.ToString();
            }

            return Guid.NewGuid().ToString("N");
        }
    }
}
