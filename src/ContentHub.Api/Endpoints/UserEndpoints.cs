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
using ContentHub.Application.Users.Commands.VerifyEmail;
using ContentHub.Application.Users.Queries.GetUserProfile;
using Microsoft.AspNetCore.Mvc;
using ContentHub.Api.Security;

namespace ContentHub.Api.Endpoints
{
    public static class UserEndpoints
    {
        public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api").WithTags("Users").RequireRateLimiting("general");

            group.MapPost("/users", async (
                [FromBody] CreateUserRequest request,
                [FromServices] CreateUserHandler handler) =>
            {
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
                    return ApiResults.Unauthorized("Invalid credentials.");
                }

                throttle.RegisterSuccess(key);
                return result.IsSuccess
                    ? Results.Ok(new AuthResponse(
                        result.Value!.UserId,
                        result.Value.AccessToken,
                        result.Value.RefreshToken,
                        result.Value.Role))
                    : ApiResults.Unauthorized("Invalid credentials.");
            }).RequireRateLimiting("auth");

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
            }).RequireRateLimiting("auth");

            group.MapPost("/auth/forgot-password", async (
                [FromBody] ForgotPasswordRequest request,
                [FromServices] ForgotPasswordHandler handler) =>
            {
                await handler.HandleAsync(new ForgotPasswordCommand(request.Email));
                return Results.Ok(new { message = "If the account exists, a reset email has been sent." });
            }).RequireRateLimiting("auth");

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
                    profile.Role,
                    profile.CreatedAtUtc,
                    profile.LastLoginAtUtc));
            }).RequireAuthorization();

            return app;
        }
    }
}
