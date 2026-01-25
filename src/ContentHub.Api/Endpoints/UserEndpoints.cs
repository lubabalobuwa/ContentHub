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
                    : Results.BadRequest(new { error = result.Error });
            });

            group.MapPost("/auth/login", async (
                [FromBody] LoginRequest request,
                [FromServices] AuthenticateUserHandler handler) =>
            {
                var result = await handler.HandleAsync(
                    new AuthenticateUserCommand(request.Email, request.Password));

                return result.IsSuccess
                    ? Results.Ok(new AuthResponse(result.Value!.UserId, result.Value.Token, result.Value.Role))
                    : Results.Unauthorized();
            });

            group.MapPost("/auth/reset-password", async (
                [FromBody] ResetPasswordRequest request,
                [FromServices] ResetPasswordHandler handler,
                [FromServices] IConfiguration config) =>
            {
                var resetKey = config["Auth:ResetKey"];
                if (string.IsNullOrWhiteSpace(resetKey) || request.ResetKey != resetKey)
                    return Results.Unauthorized();

                var result = await handler.HandleAsync(
                    new ResetPasswordCommand(request.Email, request.NewPassword));

                return result.IsSuccess
                    ? Results.Ok(new { message = "Password reset successfully." })
                    : Results.BadRequest(new { error = result.Error });
            });

            group.MapGet("/users/me", async (
                [FromServices] ICurrentUserService currentUser,
                [FromServices] GetUserProfileHandler handler) =>
            {
                if (!currentUser.IsAuthenticated || currentUser.UserId is null)
                    return Results.Unauthorized();

                var profile = await handler.HandleAsync(new GetUserProfileQuery(currentUser.UserId.Value));
                if (profile is null)
                    return Results.NotFound();

                return Results.Ok(new UserProfileResponse(
                    profile.Id,
                    profile.Email,
                    profile.DisplayName,
                    profile.Role));
            }).RequireAuthorization();

            return app;
        }
    }
}
