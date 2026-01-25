using ContentHub.Api.Contracts.Requests;
using ContentHub.Api.Contracts.Responses;
using ContentHub.Application.Users.Commands.AuthenticateUser;
using ContentHub.Application.Users.Commands.CreateUser;
using Microsoft.AspNetCore.Mvc;

namespace ContentHub.Api.Endpoints
{
    public static class UserEndpoints
    {
        public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/users", async (
                [FromBody] CreateUserRequest request,
                [FromServices] CreateUserHandler handler) =>
            {
                var result = await handler.HandleAsync(
                    new CreateUserCommand(request.Email, request.DisplayName, request.Password));

                return result.IsSuccess
                    ? Results.Created($"/api/users/{result.Value}", new { id = result.Value })
                    : Results.BadRequest(new { error = result.Error });
            });

            app.MapPost("/api/auth/login", async (
                [FromBody] LoginRequest request,
                [FromServices] AuthenticateUserHandler handler) =>
            {
                var result = await handler.HandleAsync(
                    new AuthenticateUserCommand(request.Email, request.Password));

                return result.IsSuccess
                    ? Results.Ok(new AuthResponse(result.Value!.UserId, result.Value.Token, result.Value.Role))
                    : Results.Unauthorized();
            });

            return app;
        }
    }
}
