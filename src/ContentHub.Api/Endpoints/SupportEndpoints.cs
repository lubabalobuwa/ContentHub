using ContentHub.Api.Contracts.Requests;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace ContentHub.Api.Endpoints
{
    public static class SupportEndpoints
    {
        public static IEndpointRouteBuilder MapSupportEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/support").WithTags("Support").RequireRateLimiting("general");

            group.MapPost("/contact", async (
                [FromBody] ContactSupportRequest request,
                [FromServices] IEmailSender emailSender,
                [FromServices] TurnstileVerifier turnstile,
                HttpContext httpContext) =>
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                    return ApiResults.ValidationProblem("Message is required.");

                var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();
                var turnstileResult = await turnstile.VerifyAsync(request.TurnstileToken, remoteIp);
                if (!turnstileResult.IsSuccess)
                    return ApiResults.ValidationProblem(turnstileResult.Error);

                var subject = string.IsNullOrWhiteSpace(request.Topic)
                    ? "Support request"
                    : request.Topic.Trim();

                var body = new StringBuilder()
                    .AppendLine("New support request from TechContentHub")
                    .AppendLine()
                    .AppendLine($"Name: {request.Name?.Trim()}")
                    .AppendLine($"Email: {request.Email?.Trim()}")
                    .AppendLine()
                    .AppendLine("Message:")
                    .AppendLine(WebUtility.HtmlEncode(request.Message.Trim()))
                    .ToString();

                await emailSender.SendAsync("support@techcontenthub.live", subject, body.Replace("\n", "<br/>"));

                var warning = (string?)null;
                var recipientEmail = request.Email?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(recipientEmail) && IsValidEmail(recipientEmail))
                {
                    var ackBody = new StringBuilder()
                        .AppendLine("<p>Thanks for reaching out to TechContentHub.</p>")
                        .AppendLine("<p>We’ve received your query and will respond within 24–48 hours.</p>")
                        .AppendLine("<p>If you have more details to add, reply to this email.</p>")
                        .AppendLine("<p>— TechContentHub Support</p>")
                        .ToString();

                    await emailSender.SendAsync(recipientEmail, "We received your request", ackBody);
                }
                else if (!string.IsNullOrWhiteSpace(recipientEmail))
                {
                    warning = "Acknowledgement email could not be sent due to an invalid email address.";
                }

                return Results.Ok(new { message = "Support request sent.", warning });
            }).RequireRateLimiting("external_auth");

            return app;
        }

        private static bool IsValidEmail(string inputEmail)
        {
            const string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            var re = new Regex(strRegex);
            return re.IsMatch(inputEmail);
        }
    }
}
