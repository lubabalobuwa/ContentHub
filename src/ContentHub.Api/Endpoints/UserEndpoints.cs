using ContentHub.Api.Contracts.Requests;
using ContentHub.Api.Contracts.Responses;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Application.Users.Commands.AuthenticateUser;
using ContentHub.Application.Users.Commands.CreateUser;
using ContentHub.Application.Users.Commands.ForgotPassword;
using ContentHub.Application.Users.Commands.RefreshToken;
using ContentHub.Application.Users.Commands.ResetPasswordWithToken;
using ContentHub.Application.Users.Commands.ResendVerification;
using ContentHub.Application.Users.Commands.ResetPassword;
using ContentHub.Application.Users.Commands.SetUserStatus;
using ContentHub.Application.Users.Commands.VerifyEmail;
using ContentHub.Application.Users.Queries.GetUsers;
using ContentHub.Application.Users.Queries.GetUserProfile;
using Microsoft.AspNetCore.Mvc;
using ContentHub.Api.Security;
using System;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using ContentHub.Application.Common.Options;
using ContentHub.Application.Users.ExternalLogin;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using ContentHub.Api.Services;

namespace ContentHub.Api.Endpoints
{
    public static class UserEndpoints
    {
        public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api").WithTags("Users").RequireRateLimiting("general");
            var adminGroup = app.MapGroup("/api/admin").WithTags("Admin").RequireRateLimiting("general");

            group.MapPost("/users", async (
                [FromBody] CreateUserRequest request,
                [FromServices] CreateUserHandler handler,
                [FromServices] TurnstileVerifier turnstile,
                HttpContext httpContext) =>
            {
                var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();
                var turnstileResult = await turnstile.VerifyAsync(request.TurnstileToken, remoteIp);
                if (!turnstileResult.IsSuccess)
                    return ApiResults.ValidationProblem(turnstileResult.Error);

                var result = await handler.HandleAsync(
                    new CreateUserCommand(request.Email, request.DisplayName, request.Password));

                return result.IsSuccess
                    ? Results.Created($"/api/users/{result.Value}", new { id = result.Value })
                    : ApiResults.ValidationProblem(result.Error);
            });

            group.MapPost("/auth/login", async (
                HttpContext httpContext,
                [FromBody] LoginRequest request,
                [FromServices] AuthenticateUserHandler handler,
                [FromServices] LoginThrottle throttle) =>
            {
                var emailKey = request.Email?.Trim().ToLowerInvariant() ?? string.Empty;
                var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var key = $"login:{ip}:{emailKey}";

                if (throttle.IsLockedOut(key))
                    return ApiResults.TooManyRequests("Too many login attempts. Try again later.");

                var result = await handler.HandleAsync(
                    new AuthenticateUserCommand(request.Email, request.Password));

                if (!result.IsSuccess)
                {
                    throttle.RegisterFailure(key);
                    return result.Error switch
                    {
                        "User disabled." => ApiResults.Forbidden("User disabled. Contact support."),
                        "Email not verified." => ApiResults.Forbidden("Email not verified. Check your inbox."),
                        _ => ApiResults.Unauthorized("Invalid credentials.")
                    };
                }

                throttle.RegisterSuccess(key);
                return result.IsSuccess
                    ? Results.Ok(new AuthResponse(
                        result.Value!.UserId,
                        result.Value.AccessToken,
                        result.Value.RefreshToken,
                        result.Value.Role))
                    : ApiResults.Unauthorized("Invalid credentials.");
            }).RequireRateLimiting("external_auth");

            group.MapPost("/auth/refresh", async (
                [FromBody] RefreshTokenRequest request,
                [FromServices] RefreshTokenHandler handler) =>
            {
                var result = await handler.HandleAsync(
                    new RefreshTokenCommand(request.RefreshToken));

                return result.IsSuccess
                    ? Results.Ok(new AuthResponse(
                        result.Value!.UserId,
                        result.Value.AccessToken,
                        result.Value.RefreshToken,
                        result.Value.Role))
                    : ApiResults.Unauthorized("Invalid refresh token.");
            });

            group.MapGet("/auth/verify-email", async (
                [FromQuery] string token,
                [FromServices] VerifyEmailHandler handler) =>
            {
                var result = await handler.HandleAsync(new VerifyEmailCommand(token));
                return result.IsSuccess
                    ? Results.Ok(new { message = "Email verified successfully." })
                    : ApiResults.ValidationProblem(result.Error);
            });

            group.MapPost("/auth/resend-verification", async (
                [FromBody] ResendVerificationRequest request,
                [FromServices] ResendVerificationHandler handler) =>
            {
                await handler.HandleAsync(new ResendVerificationCommand(request.Email));
                return Results.Ok(new { message = "If the account exists, a verification email has been sent." });
            }).RequireRateLimiting("external_auth");

            group.MapPost("/auth/forgot-password", async (
                [FromBody] ForgotPasswordRequest request,
                [FromServices] ForgotPasswordHandler handler) =>
            {
                await handler.HandleAsync(new ForgotPasswordCommand(request.Email));
                return Results.Ok(new { message = "If the account exists, a reset email has been sent." });
            }).RequireRateLimiting("external_auth");

            group.MapPost("/auth/reset-password", async (
                [FromBody] ResetPasswordWithTokenRequest request,
                [FromServices] ResetPasswordWithTokenHandler handler) =>
            {
                var result = await handler.HandleAsync(
                    new ResetPasswordWithTokenCommand(request.Token, request.NewPassword));

                return result.IsSuccess
                    ? Results.Ok(new { message = "Password reset successfully." })
                    : ApiResults.ValidationProblem(result.Error);
            }).RequireRateLimiting("auth");

            group.MapPost("/auth/admin/reset-password", async (
                [FromBody] ResetPasswordRequest request,
                [FromServices] ResetPasswordHandler handler,
                [FromServices] IConfiguration config,
                [FromServices] ICurrentUserService currentUser) =>
            {
                var enabled = config.GetValue<bool>("Auth:EnableResetPassword");
                if (!enabled)
                    return ApiResults.NotFound();

                if (!currentUser.IsAuthenticated || currentUser.UserId is null)
                    return ApiResults.Unauthorized();

                if (currentUser.Role != ContentHub.Domain.Users.UserRole.Admin)
                    return ApiResults.Forbidden();

                var result = await handler.HandleAsync(
                    new ResetPasswordCommand(request.Email, request.NewPassword));

                return result.IsSuccess
                    ? Results.Ok(new { message = "Password reset successfully." })
                    : ApiResults.ValidationProblem(result.Error);
            }).RequireAuthorization();

            group.MapGet("/auth/external/google", async(
                HttpContext httpContext,
                [FromServices] IAuthenticationSchemeProvider schemeProvider) =>
            {
                var scheme = await schemeProvider.GetSchemeAsync("Google");
                if (scheme is null)
                    return ApiResults.NotFound("Google authentication is not configured.");

                var properties = new AuthenticationProperties
                {
                    RedirectUri = "/api/auth/external/google/complete"
                };

                return Results.Challenge(properties, new[] { "Google" });
            }).RequireRateLimiting("auth");

            group.MapGet("/auth/external/google/complete", async (
                HttpContext httpContext,
                [FromServices] ExternalLoginHandler handler,
                [FromServices] IOptions<AuthSettings> authOptions,
                [FromServices] IExternalAuthCodeStore codeStore) =>
            {
                var authResult = await httpContext.AuthenticateAsync("External");
                if (!authResult.Succeeded || authResult.Principal is null)
                    return ApiResults.Unauthorized("External authentication failed.");

                var principal = authResult.Principal;
                var providerUserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? principal.FindFirst("sub")?.Value
                    ?? string.Empty;

                var email = principal.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
                var displayName = principal.FindFirst(ClaimTypes.Name)?.Value ?? email;
                var emailVerifiedClaim = principal.FindFirst("email_verified")?.Value ?? string.Empty;
                var emailVerified = string.Equals(emailVerifiedClaim, "true", StringComparison.OrdinalIgnoreCase);

                var result = await handler.HandleAsync(new ExternalLoginCommand(
                    "Google",
                    providerUserId,
                    email,
                    displayName,
                    emailVerified));

                if (!result.IsSuccess)
                {
                    return result.Error switch
                    {
                        "User disabled." => ApiResults.Forbidden("User disabled. Contact support."),
                        _ => ApiResults.ValidationProblem(result.Error)
                    };
                }

                await httpContext.SignOutAsync("External");

                var baseUrl = authOptions.Value.BaseUrl?.TrimEnd('/') ?? string.Empty;
                if (string.IsNullOrWhiteSpace(baseUrl))
                {
                    return Results.Ok(new AuthResponse(
                        result.Value!.UserId,
                        result.Value.AccessToken,
                        result.Value.RefreshToken,
                        result.Value.Role));
                }

                var code = Guid.NewGuid().ToString("N");
                await codeStore.StoreAsync(
                    code,
                    new AuthResponse(
                        result.Value!.UserId,
                        result.Value.AccessToken,
                        result.Value.RefreshToken,
                        result.Value.Role),
                    TimeSpan.FromMinutes(2));

                var redirectUrl = $"{baseUrl}/external-auth?code={Uri.EscapeDataString(code)}";

                return Results.Redirect(redirectUrl);

            }).RequireRateLimiting("auth");

            group.MapPost("/auth/external/exchange", async (
                [FromBody] ExternalAuthExchangeRequest request,
                [FromServices] IExternalAuthCodeStore codeStore) =>
            {
                if (string.IsNullOrWhiteSpace(request.Code))
                    return ApiResults.ValidationProblem("Code is required.");

                var response = await codeStore.TryConsumeAsync(request.Code);
                if (response is null)
                    return ApiResults.Unauthorized("Invalid or expired code.");

                return Results.Ok(response);
            }).RequireRateLimiting("auth");

            group.MapGet("/users/me", async (
                [FromServices] ICurrentUserService currentUser,
                [FromServices] GetUserProfileHandler handler) =>
            {
                if (!currentUser.IsAuthenticated || currentUser.UserId is null)
                    return ApiResults.Unauthorized();

                var profile = await handler.HandleAsync(new GetUserProfileQuery(currentUser.UserId.Value));
                if (profile is null)
                    return ApiResults.NotFound("User not found.");

                return Results.Ok(new UserProfileResponse(
                    profile.Id,
                    profile.Email,
                    profile.DisplayName,
                    profile.ProfileImageUrl,
                    profile.Role,
                    profile.CreatedAtUtc,
                    profile.LastLoginAtUtc));
            }).RequireAuthorization();

            adminGroup.MapGet("/users", async (
                int? page,
                int? pageSize,
                string? search,
                [FromServices] GetUsersHandler handler,
                [FromServices] ICurrentUserService currentUser) =>
            {
                if (!currentUser.IsAuthenticated)
                    return ApiResults.Unauthorized();

                if (currentUser.Role != ContentHub.Domain.Users.UserRole.Admin)
                    return ApiResults.Forbidden();

                if (!TryNormalizePaging(page, pageSize, out var normalizedPage, out var normalizedPageSize, out var error))
                    return error!;

                var users = await handler.HandleAsync(new GetUsersQuery(normalizedPage, normalizedPageSize, search));

                var items = users.Items.Select(x => new AdminUserResponse(
                    x.Id,
                    x.Email,
                    x.DisplayName,
                    x.Role,
                    x.EmailConfirmed,
                    x.IsDisabled,
                    x.CreatedAtUtc,
                    x.LastLoginAtUtc)).ToList();

                return Results.Ok(new PagedResponse<AdminUserResponse>(
                    items,
                    users.Page,
                    users.PageSize,
                    users.TotalCount,
                    users.TotalPages));
            }).RequireAuthorization();

            adminGroup.MapPut("/users/{id:guid}/status", async (
                Guid id,
                [FromBody] UpdateUserStatusRequest request,
                [FromServices] SetUserStatusHandler handler) =>
            {
                var result = await handler.HandleAsync(new SetUserStatusCommand(id, request.IsDisabled));

                return result.IsSuccess
                    ? Results.Ok(new { message = "User status updated." })
                    : result.Error switch
                    {
                        "Unauthorized." => ApiResults.Unauthorized(),
                        "Forbidden." => ApiResults.Forbidden(),
                        "User not found." => ApiResults.NotFound("User not found."),
                        _ => ApiResults.ValidationProblem(result.Error)
                    };
            }).RequireAuthorization();

            return app;
        }

        private static bool TryNormalizePaging(
            int? page,
            int? pageSize,
            out int normalizedPage,
            out int normalizedPageSize,
            out IResult? error)
        {
            const int defaultPage = 1;
            const int defaultPageSize = 20;
            const int maxPageSize = 100;

            normalizedPage = page ?? defaultPage;
            normalizedPageSize = pageSize ?? defaultPageSize;

            if (normalizedPage < 1)
            {
                error = ApiResults.ValidationProblem("Page must be greater than or equal to 1.");
                return false;
            }

            if (normalizedPageSize < 1 || normalizedPageSize > maxPageSize)
            {
                error = ApiResults.ValidationProblem("PageSize must be between 1 and 100.");
                return false;
            }

            error = null;
            return true;
        }
    }
}
