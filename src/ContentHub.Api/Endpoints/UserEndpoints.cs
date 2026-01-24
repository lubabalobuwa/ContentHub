using ContentHub.Api.Contracts.Requests;
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
                var result = await handler.HandleAsync(new CreateUserCommand(request.Email, request.DisplayName));

                return result.IsSuccess
                    ? Results.Created($"/api/users/{result.Value}", new { id = result.Value })
                    : Results.BadRequest(new { error = result.Error });
            });

            return app;
        }
    }
}