using Microsoft.AspNetCore.Mvc;

namespace ContentHub.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
            {
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status409Conflict,
                    "Conflict",
                    "Content was modified by another request. Please refresh and try again.",
                    ex.Message);
            }
            catch (Exception ex)
            {
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "Unexpected error",
                    ex.Message,
                    null);
            }
        }

        private static Task WriteProblemAsync(
            HttpContext context,
            int statusCode,
            string title,
            string detail,
            string? debugDetails)
        {
            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path
            };

            problem.Extensions["traceId"] = context.TraceIdentifier;
            if (!string.IsNullOrWhiteSpace(debugDetails))
                problem.Extensions["debug"] = debugDetails;

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";
            return context.Response.WriteAsJsonAsync(problem);
        }
    }
}
