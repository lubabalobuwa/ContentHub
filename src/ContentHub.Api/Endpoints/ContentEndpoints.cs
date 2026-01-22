using ContentHub.Api.Contracts.Requests;
using ContentHub.Api.Contracts.Responses;
using ContentHub.Application.Content.Commands.CreateContent;
using ContentHub.Application.Content.Queries.GetContentById;
using ContentHub.Application.Content.Queries.GetPublishedContent;
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
                    ? Results.Created($"/api/content", new { messahe = "Content created successfully" }) 
                    : Results.BadRequest(new { error = result.Error });
            });

            app.MapGet("/api/content/{id:guid}", async (Guid id, [FromServices] GetContentByIdHandler handler) =>
            {
                var content = await handler.HandleAsync(new GetContentByIdQuery(id));

                if (content is null)
                    return Results.NotFound();

                return Results.Ok(new ContentSummaryResponse(
                    content.Id,
                    content.Title,
                    content.Body,
                    content.Status.ToString()
                ));
            });

            app.MapGet("/api/content", async ([FromServices] GetPublishedContentHandler handler) =>
            {
                var content = await handler.HandleAsync();

                return Results.Ok(content.Select(x =>
                    new ContentSummaryResponse(
                        x.Id,
                        x.Title,
                        x.Body,
                        x.Status.ToString())
                ));
            });

            return app;
        }
    }
}
