using ContentHub.Api.Contracts.Requests;
using ContentHub.Application.Content.Commands.CreateContent;
using Microsoft.AspNetCore.Mvc;

namespace ContentHub.Api.Endpoints
{
    public static class ContentEndpoints
    {
        public static IEndpointRouteBuilder MapContentEndpoints(this  IEndpointRouteBuilder app)
        {
            app.MapPost("/api/content", async ([FromBody] CreateContentRequest request, CreateContentHandler handler) =>
            {
                var command = new CreateContentCommand(
                    request.Title,
                    request.Body,
                    request.AuthorId);

                var result = await handler.HandleAsync(command);

                return result.IsSuccess 
                    ? Results.Created($"/api/content", null) 
                    : Results.BadRequest(result.Error);
            });

            return app;
        }
    }
}
