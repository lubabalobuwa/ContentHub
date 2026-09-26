using ContentHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Endpoints
{
    public static class HealthEndpoints
    {
        public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/health/db", async (ContentHubDbContext db) =>
            {
                var canConnect = await db.Database.CanConnectAsync();

                return canConnect
                    ? Results.Ok(new { status = "warm", timestamp = DateTime.UtcNow })
                    : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            })
            .WithName("DatabaseHealthCheck")
            .AllowAnonymous();

            return app;
        }
    }
}
