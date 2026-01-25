using ContentHub.Api.Contracts.Requests;
using ContentHub.Api.Contracts.Responses;
using ContentHub.Application.Content.Commands.ArchiveContent;
using ContentHub.Application.Content.Commands.CreateContent;
using ContentHub.Application.Content.Commands.DeleteContent;
using ContentHub.Application.Content.Commands.PublishContent;
using ContentHub.Application.Content.Commands.RestoreContent;
using ContentHub.Application.Content.Commands.UpdateContent;
using ContentHub.Application.Content.Queries.GetArchivedContent;
using ContentHub.Application.Content.Queries.GetArchivedContentByAuthor;
using ContentHub.Application.Content.Queries.GetContentById;
using ContentHub.Application.Content.Queries.GetDraftContent;
using ContentHub.Application.Content.Queries.GetDraftContentByAuthor;
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

            app.MapPut("/api/content/{id:guid}", async (Guid id, [FromBody] UpdateContentRequest request, UpdateContentHandler handler) =>
            {
                var command = new UpdateContentCommand(id, request.Title, request.Body);
                var result = await handler.HandleAsync(command);

                return result.IsSuccess
                    ? Results.Ok(new { message = "Content updated successfully" })
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

            // TODO: Remove global drafts/archived endpoints once auth is enforced.
            app.MapGet("/api/content/drafts", async ([FromServices] GetDraftContentHandler handler) =>
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

            app.MapGet("/api/content/authors/{authorId:guid}/drafts", async (Guid authorId, [FromServices] GetDraftContentByAuthorHandler handler) =>
            {
                if (authorId == Guid.Empty)
                    return Results.BadRequest(new { error = "AuthorId is required." });

                var content = await handler.HandleAsync(new GetDraftContentByAuthorQuery(authorId));

                return Results.Ok(content.Select(x =>
                    new ContentSummaryResponse(
                        x.Id,
                        x.Title,
                        x.Body,
                        x.Status.ToString())
                ));
            });

            app.MapGet("/api/content/archived", async ([FromServices] GetArchivedContentHandler handler) =>
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

            app.MapGet("/api/content/authors/{authorId:guid}/archived", async (Guid authorId, [FromServices] GetArchivedContentByAuthorHandler handler) =>
            {
                if (authorId == Guid.Empty)
                    return Results.BadRequest(new { error = "AuthorId is required." });

                var content = await handler.HandleAsync(new GetArchivedContentByAuthorQuery(authorId));

                return Results.Ok(content.Select(x =>
                    new ContentSummaryResponse(
                        x.Id,
                        x.Title,
                        x.Body,
                        x.Status.ToString())
                ));
            });

            app.MapPost("/api/content/{id:guid}/publish", async (Guid id, [FromServices] PublishContentHandler handler) =>
            {
                var result = await handler.HandleAsync(new PublishContentCommand(id));

                return result.IsSuccess
                    ? Results.Ok(new { message = "Content published." })
                    : Results.BadRequest(new { error = result.Error });
            });

            app.MapPost("/api/content/{id:guid}/archive", async (Guid id, [FromServices] ArchiveContentHandler handler) =>
            {
                var result = await handler.HandleAsync(new ArchiveContentCommand(id));

                return result.IsSuccess
                    ? Results.Ok(new { message = "Content archived." })
                    : Results.BadRequest(new { error = result.Error });
            });

            app.MapPost("/api/content/{id:guid}/restore", async (Guid id, [FromServices] RestoreContentHandler handler) =>
            {
                var result = await handler.HandleAsync(new RestoreContentCommand(id));

                return result.IsSuccess
                    ? Results.Ok(new { message = "Content restored." })
                    : Results.BadRequest(new { error = result.Error });
            });

            app.MapDelete("/api/content/{id:guid}", async (Guid id, [FromServices] DeleteContentHandler handler) =>
            {
                var result = await handler.HandleAsync(new DeleteContentCommand(id));

                return result.IsSuccess
                    ? Results.Ok(new { message = "Content deleted." })
                    : Results.BadRequest(new { error = result.Error });
            });

            return app;
        }
    }
}
