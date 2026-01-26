using ContentHub.Api.Contracts.Requests;
using ContentHub.Api.Contracts.Responses;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Application.Users.Commands.AuthenticateUser;
using ContentHub.Application.Users.Commands.CreateUser;
using ContentHub.Application.Users.Commands.ResetPassword;
using ContentHub.Application.Users.Queries.GetUserProfile;
using Microsoft.AspNetCore.Mvc;

namespace ContentHub.Api.Endpoints
{
    public static class UserEndpoints
    {
        public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api").WithTags("Users");

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
                [FromBody] LoginRequest request,
                [FromServices] AuthenticateUserHandler handler) =>
            {
                var result = await handler.HandleAsync(
                    new AuthenticateUserCommand(request.Email, request.Password));

                return result.IsSuccess
                    ? Results.Ok(new AuthResponse(result.Value!.UserId, result.Value.Token, result.Value.Role))
                    : ApiResults.Unauthorized("Invalid credentials.");
            });

            group.MapPost("/auth/reset-password", async (
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
