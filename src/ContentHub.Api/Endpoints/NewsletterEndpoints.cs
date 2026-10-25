using System.Net.Mail;
using ContentHub.Api.Contracts.Requests;
using ContentHub.Domain.Newsletter;
using ContentHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContentHub.Api.Endpoints;

public static class NewsletterEndpoints
{
    public static IEndpointRouteBuilder MapNewsletterEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/newsletter")
            .WithTags("Newsletter")
            .RequireRateLimiting("general");

        group.MapPost("/subscribe", async (
            [FromBody] NewsletterSubscribeRequest request,
            [FromServices] ContentHubDbContext dbContext) =>
        {
            var email = request.Email?.Trim() ?? string.Empty;
            if (!MailAddress.TryCreate(email, out var parsedAddress) ||
                !string.Equals(parsedAddress.Address, email, StringComparison.OrdinalIgnoreCase))
            {
                return ApiResults.ValidationProblem("Enter a valid email address.");
            }

            var normalizedEmail = email.ToUpperInvariant();
            var existing = await dbContext.NewsletterSubscriptions
                .SingleOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail);

            if (existing is not null)
            {
                if (!existing.IsActive)
                {
                    existing.Reactivate(email, DateTime.UtcNow);
                    await dbContext.SaveChangesAsync();
                }

                return Results.Ok(new { message = "You are subscribed to new article alerts." });
            }

            dbContext.NewsletterSubscriptions.Add(new NewsletterSubscription(email, DateTime.UtcNow));

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // A concurrent request may have inserted the same normalized address.
            }

            return Results.Ok(new { message = "You are subscribed to new article alerts." });
        });

        return app;
    }
}
