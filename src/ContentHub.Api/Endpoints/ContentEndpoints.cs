using ContentHub.Api.Contracts.Requests;
using ContentHub.Api.Contracts.Responses;
using ContentHub.Application.Common.Interfaces;
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
using ContentHub.Domain.Content;
using ContentHub.Domain.Users;
using Microsoft.AspNetCore.Mvc;

namespace ContentHub.Api.Endpoints
{
    public static class ContentEndpoints
    {
        public static IEndpointRouteBuilder MapContentEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/content").WithTags("Content");

            group.MapPost("", async (
                [FromBody] CreateContentRequest request,
                CreateContentHandler handler,
                ICurrentUserService currentUser) =>
            {
                if (!currentUser.IsAuthenticated || currentUser.UserId is null)
                    return Results.Unauthorized();

                var command = new CreateContentCommand(
                    request.Title,
                    request.Body,
                    currentUser.UserId.Value);

                var result = await handler.HandleAsync(command);

                return result.IsSuccess
                    ? Results.Created("/api/content", new { message = "Content created successfully" })
                    : MapFailure(result.Error);
            }).RequireAuthorization();

            group.MapPut("/{id:guid}", async (Guid id, [FromBody] UpdateContentRequest request, UpdateContentHandler handler) =>
            {
                var command = new UpdateContentCommand(id, request.Title, request.Body, request.RowVersion);
                var result = await handler.HandleAsync(command);

                return result.IsSuccess
                    ? Results.Ok(new { message = "Content updated successfully" })
                    : MapFailure(result.Error);
            }).RequireAuthorization();

            group.MapGet("/{id:guid}", async (
                Guid id,
                [FromServices] GetContentByIdHandler handler,
                [FromServices] ICurrentUserService currentUser) =>
            {
                var content = await handler.HandleAsync(new GetContentByIdQuery(id));

                if (content is null)
                    return Results.NotFound();

                if (content.Status != ContentStatus.Published)
                {
                    if (!currentUser.IsAuthenticated || currentUser.UserId is null)
                        return Results.NotFound();

                    if (currentUser.Role != UserRole.Admin && currentUser.UserId.Value != content.AuthorId)
                        return Results.Forbid();
                }

                return Results.Ok(new ContentSummaryResponse(
                    content.Id,
                    content.Title,
                    content.Body,
                    content.Status.ToString(),
                    Convert.ToBase64String(content.RowVersion)
                ));
            });

            group.MapGet("", async ([FromServices] GetPublishedContentHandler handler) =>
            {
                var content = await handler.HandleAsync();

                return Results.Ok(content.Select(x =>
                    new ContentSummaryResponse(
                        x.Id,
                        x.Title,
                        x.Body,
                        x.Status.ToString(),
                        Convert.ToBase64String(x.RowVersion))
                ));
            });

            // TODO: Remove global drafts/archived endpoints once auth is enforced.
            group.MapGet("/drafts", async (
                [FromServices] GetDraftContentHandler handler,
                [FromServices] ICurrentUserService currentUser) =>
            {
                if (!currentUser.IsAuthenticated)
                    return Results.Unauthorized();

                if (currentUser.Role != UserRole.Admin)
                    return Results.Forbid();

                var content = await handler.HandleAsync();

                return Results.Ok(content.Select(x =>
                    new ContentSummaryResponse(
                        x.Id,
                        x.Title,
                        x.Body,
                        x.Status.ToString(),
                        Convert.ToBase64String(x.RowVersion))
                ));
            }).RequireAuthorization();

            group.MapGet("/authors/{authorId:guid}/drafts", async (
                Guid authorId,
                [FromServices] GetDraftContentByAuthorHandler handler,
                [FromServices] ICurrentUserService currentUser) =>
            {
                if (authorId == Guid.Empty)
                    return Results.BadRequest(new { error = "AuthorId is required." });

                if (!currentUser.IsAuthenticated || currentUser.UserId is null)
                    return Results.Unauthorized();

                if (currentUser.Role != UserRole.Admin && currentUser.UserId.Value != authorId)
                    return Results.Forbid();

                var content = await handler.HandleAsync(new GetDraftContentByAuthorQuery(authorId));

                return Results.Ok(content.Select(x =>
                    new ContentSummaryResponse(
                        x.Id,
                        x.Title,
                        x.Body,
                        x.Status.ToString(),
                        Convert.ToBase64String(x.RowVersion))
                ));
            }).RequireAuthorization();

            group.MapGet("/archived", async (
                [FromServices] GetArchivedContentHandler handler,
                [FromServices] ICurrentUserService currentUser) =>
            {
                if (!currentUser.IsAuthenticated)
                    return Results.Unauthorized();

                if (currentUser.Role != UserRole.Admin)
                    return Results.Forbid();

                var content = await handler.HandleAsync();

                return Results.Ok(content.Select(x =>
                    new ContentSummaryResponse(
                        x.Id,
                        x.Title,
                        x.Body,
                        x.Status.ToString(),
                        Convert.ToBase64String(x.RowVersion))
                ));
            }).RequireAuthorization();

            group.MapGet("/authors/{authorId:guid}/archived", async (
                Guid authorId,
                [FromServices] GetArchivedContentByAuthorHandler handler,
                [FromServices] ICurrentUserService currentUser) =>
            {
                if (authorId == Guid.Empty)
                    return Results.BadRequest(new { error = "AuthorId is required." });

                if (!currentUser.IsAuthenticated || currentUser.UserId is null)
                    return Results.Unauthorized();

                if (currentUser.Role != UserRole.Admin && currentUser.UserId.Value != authorId)
                    return Results.Forbid();

                var content = await handler.HandleAsync(new GetArchivedContentByAuthorQuery(authorId));

                return Results.Ok(content.Select(x =>
                    new ContentSummaryResponse(
                        x.Id,
                        x.Title,
                        x.Body,
                        x.Status.ToString(),
                        Convert.ToBase64String(x.RowVersion))
                ));
            }).RequireAuthorization();

            group.MapPost("/{id:guid}/publish", async (
                Guid id,
                [FromBody] ConcurrencyRequest request,
                [FromServices] PublishContentHandler handler) =>
            {
                var result = await handler.HandleAsync(new PublishContentCommand(id, request.RowVersion));

                return result.IsSuccess
                    ? Results.Ok(new { message = "Content published." })
                    : MapFailure(result.Error);
            }).RequireAuthorization();

            group.MapPost("/{id:guid}/archive", async (
                Guid id,
                [FromBody] ConcurrencyRequest request,
                [FromServices] ArchiveContentHandler handler) =>
            {
                var result = await handler.HandleAsync(new ArchiveContentCommand(id, request.RowVersion));

                return result.IsSuccess
                    ? Results.Ok(new { message = "Content archived." })
                    : MapFailure(result.Error);
            }).RequireAuthorization();

            group.MapPost("/{id:guid}/restore", async (
                Guid id,
                [FromBody] ConcurrencyRequest request,
                [FromServices] RestoreContentHandler handler) =>
            {
                var result = await handler.HandleAsync(new RestoreContentCommand(id, request.RowVersion));

                return result.IsSuccess
                    ? Results.Ok(new { message = "Content restored." })
                    : MapFailure(result.Error);
            }).RequireAuthorization();

            group.MapDelete("/{id:guid}", async (
                Guid id,
                [FromBody] ConcurrencyRequest request,
                [FromServices] DeleteContentHandler handler) =>
            {
                var result = await handler.HandleAsync(new DeleteContentCommand(id, request.RowVersion));

                return result.IsSuccess
                    ? Results.Ok(new { message = "Content deleted." })
                    : MapFailure(result.Error);
            }).RequireAuthorization();

            return app;
        }

        private static IResult MapFailure(string error)
        {
            return error switch
            {
                "Unauthorized." => Results.Unauthorized(),
                "Forbidden." => Results.Forbid(),
                "Content not found." => Results.NotFound(),
                "Author not found." => Results.NotFound(),
                _ => Results.BadRequest(new { error })
            };
        }
    }
}
