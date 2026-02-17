using ContentHub.Api.Contracts.Requests;
using ContentHub.Api.Contracts.Responses;
using ContentHub.Application.Comments.Commands.CreateComment;
using ContentHub.Application.Comments.Queries.GetCommentsByContentId;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Application.Content.Commands.ArchiveContent;
using ContentHub.Application.Content.Commands.CreateContent;
using ContentHub.Application.Content.Commands.DeleteContent;
using ContentHub.Application.Content.Commands.PublishContent;
using ContentHub.Application.Content.Commands.RestoreContent;
using ContentHub.Application.Content.Commands.UpdateContent;
using ContentHub.Application.Content.Queries;
using ContentHub.Application.Content.Queries.GetArchivedContent;
using ContentHub.Application.Content.Queries.GetArchivedContentByAuthor;
using ContentHub.Application.Content.Queries.GetContentById;
using ContentHub.Application.Content.Queries.GetDraftContent;
using ContentHub.Application.Content.Queries.GetDraftContentByAuthor;
using ContentHub.Application.Content.Queries.GetPublishedContent;
using ContentHub.Application.Content.Queries.GetPublishedContentByAuthor;
using ContentHub.Domain.Content;
using ContentHub.Domain.Users;
using Microsoft.AspNetCore.Mvc;

namespace ContentHub.Api.Endpoints
{
    public static class ContentEndpoints
    {
        public static IEndpointRouteBuilder MapContentEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/content").WithTags("Content").RequireRateLimiting("general");

            group.MapPost("", async (
                [FromBody] CreateContentRequest request,
                CreateContentHandler handler,
                ICurrentUserService currentUser) =>
            {
                if (!currentUser.IsAuthenticated || currentUser.UserId is null)
                    return ApiResults.Unauthorized();

                var command = new CreateContentCommand(
                    request.Title,
                    request.Body,
                    currentUser.UserId.Value);

                var result = await handler.HandleAsync(command);

                return result.IsSuccess
                    ? Results.Created($"/api/content/{result.Value}", new CreateContentResponse(result.Value))
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
                    return ApiResults.NotFound("Content not found.");

                if (content.Status != ContentStatus.Published)
                {
                    if (!currentUser.IsAuthenticated || currentUser.UserId is null)
                        return ApiResults.NotFound("Content not found.");

                    if (currentUser.Role != UserRole.Admin && currentUser.UserId.Value != content.AuthorId)
                        return ApiResults.Forbidden();
                }

                return Results.Ok(new ContentDetailResponse(
                    content.Id,
                    content.AuthorId,
                    content.AuthorDisplayName,
                    content.AuthorProfileImageUrl,
                    content.Title,
                    content.Body,
                    content.Status.ToString(),
                    content.ImageUrl,
                    content.CreatedAtUtc,
                    content.PublishedAtUtc,
                    Convert.ToBase64String(content.RowVersion)
                ));
            });

            group.MapGet("/{id:guid}/comments", async (
                Guid id,
                [FromServices] GetCommentsByContentIdHandler handler,
                [FromServices] IContentReadRepository contentReadRepository,
                [FromServices] ICurrentUserService currentUser) =>
            {
                var content = await contentReadRepository.GetByIdAsync(id);
                if (content is null)
                    return ApiResults.NotFound("Content not found.");

                if (content.Status != ContentStatus.Published)
                {
                    if (!currentUser.IsAuthenticated || currentUser.UserId is null)
                        return ApiResults.NotFound("Content not found.");

                    if (currentUser.Role != UserRole.Admin && currentUser.UserId.Value != content.AuthorId)
                        return ApiResults.Forbidden();
                }

                var comments = await handler.HandleAsync(new GetCommentsByContentIdQuery(id));
                var response = comments.Select(x =>
                    new CommentResponse(
                        x.Id,
                        x.ContentItemId,
                        x.UserId,
                        x.UserDisplayName,
                        x.UserProfileImageUrl,
                        x.Text,
                        x.CreatedAtUtc))
                    .ToList();

                return Results.Ok(response);
            });

            group.MapPost("/{id:guid}/comments", async (
                Guid id,
                [FromBody] CreateCommentRequest request,
                [FromServices] CreateCommentHandler handler) =>
            {
                var result = await handler.HandleAsync(new CreateCommentCommand(id, request.Text));

                return result.IsSuccess
                    ? Results.Created(
                        $"/api/content/{id}/comments/{result.Value.Id}",
                        new CommentResponse(
                            result.Value.Id,
                            result.Value.ContentItemId,
                            result.Value.UserId,
                            result.Value.UserDisplayName,
                            result.Value.UserProfileImageUrl,
                            result.Value.Text,
                            result.Value.CreatedAtUtc))
                    : MapFailure(result.Error);
            }).RequireAuthorization();

            group.MapGet("", async (
                int? page,
                int? pageSize,
                string? search,
                [FromServices] GetPublishedContentHandler handler) =>
            {
                if (!TryNormalizePaging(page, pageSize, out var normalizedPage, out var normalizedPageSize, out var error))
                    return error!;

                var content = await handler.HandleAsync(normalizedPage, normalizedPageSize, search);

                return Results.Ok(MapPagedResponse(content));
            });

            // TODO: Remove global drafts/archived endpoints once auth is enforced.
            group.MapGet("/drafts", async (
                int? page,
                int? pageSize,
                string? search,
                [FromServices] GetDraftContentHandler handler,
                [FromServices] ICurrentUserService currentUser) =>
            {
                if (!currentUser.IsAuthenticated)
                    return ApiResults.Unauthorized();

                if (currentUser.Role != UserRole.Admin)
                    return ApiResults.Forbidden();

                if (!TryNormalizePaging(page, pageSize, out var normalizedPage, out var normalizedPageSize, out var error))
                    return error!;

                var content = await handler.HandleAsync(normalizedPage, normalizedPageSize, search);

                return Results.Ok(MapPagedResponse(content));
            }).RequireAuthorization();

            group.MapGet("/authors/{authorId:guid}/drafts", async (
                Guid authorId,
                int? page,
                int? pageSize,
                [FromServices] GetDraftContentByAuthorHandler handler,
                [FromServices] ICurrentUserService currentUser) =>
            {
                if (authorId == Guid.Empty)
                    return ApiResults.ValidationProblem("AuthorId is required.");

                if (!currentUser.IsAuthenticated || currentUser.UserId is null)
                    return ApiResults.Unauthorized();

                if (currentUser.Role != UserRole.Admin && currentUser.UserId.Value != authorId)
                    return ApiResults.Forbidden();

                if (!TryNormalizePaging(page, pageSize, out var normalizedPage, out var normalizedPageSize, out var error))
                    return error!;

                var content = await handler.HandleAsync(
                    new GetDraftContentByAuthorQuery(authorId, normalizedPage, normalizedPageSize));

                return Results.Ok(MapPagedResponse(content));
            }).RequireAuthorization();

            group.MapGet("/authors/{authorId:guid}/published", async (
                Guid authorId,
                int? page,
                int? pageSize,
                [FromServices] GetPublishedContentByAuthorHandler handler,
                [FromServices] ICurrentUserService currentUser) =>
            {
                if (authorId == Guid.Empty)
                    return ApiResults.ValidationProblem("AuthorId is required.");

                if (!currentUser.IsAuthenticated || currentUser.UserId is null)
                    return ApiResults.Unauthorized();

                if (currentUser.Role != UserRole.Admin && currentUser.UserId.Value != authorId)
                    return ApiResults.Forbidden();

                if (!TryNormalizePaging(page, pageSize, out var normalizedPage, out var normalizedPageSize, out var error))
                    return error!;

                var content = await handler.HandleAsync(
                    new GetPublishedContentByAuthorQuery(authorId, normalizedPage, normalizedPageSize));

                return Results.Ok(MapPagedResponse(content));
            }).RequireAuthorization();

            group.MapGet("/archived", async (
                int? page,
                int? pageSize,
                string? search,
                [FromServices] GetArchivedContentHandler handler,
                [FromServices] ICurrentUserService currentUser) =>
            {
                if (!currentUser.IsAuthenticated)
                    return ApiResults.Unauthorized();

                if (currentUser.Role != UserRole.Admin)
                    return ApiResults.Forbidden();

                if (!TryNormalizePaging(page, pageSize, out var normalizedPage, out var normalizedPageSize, out var error))
                    return error!;

                var content = await handler.HandleAsync(normalizedPage, normalizedPageSize, search);

                return Results.Ok(MapPagedResponse(content));
            }).RequireAuthorization();

            group.MapGet("/authors/{authorId:guid}/archived", async (
                Guid authorId,
                int? page,
                int? pageSize,
                [FromServices] GetArchivedContentByAuthorHandler handler,
                [FromServices] ICurrentUserService currentUser) =>
            {
                if (authorId == Guid.Empty)
                    return ApiResults.ValidationProblem("AuthorId is required.");

                if (!currentUser.IsAuthenticated || currentUser.UserId is null)
                    return ApiResults.Unauthorized();

                if (currentUser.Role != UserRole.Admin && currentUser.UserId.Value != authorId)
                    return ApiResults.Forbidden();

                if (!TryNormalizePaging(page, pageSize, out var normalizedPage, out var normalizedPageSize, out var error))
                    return error!;

                var content = await handler.HandleAsync(
                    new GetArchivedContentByAuthorQuery(authorId, normalizedPage, normalizedPageSize));

                return Results.Ok(MapPagedResponse(content));
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
                "Unauthorized." => ApiResults.Unauthorized(),
                "Forbidden." => ApiResults.Forbidden(),
                "Content not found." => ApiResults.NotFound("Content not found."),
                "Author not found." => ApiResults.NotFound("Author not found."),
                "User not found." => ApiResults.NotFound("User not found."),
                "Content is not published." => ApiResults.ValidationProblem("Content is not published."),
                _ => ApiResults.ValidationProblem(error)
            };
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

        private static PagedResponse<ContentSummaryResponse> MapPagedResponse(
            ContentHub.Application.Common.PagedResult<ContentSummaryDto> content)
        {
            var items = content.Items.Select(x =>
                new ContentSummaryResponse(
                    x.Id,
                    x.AuthorId,
                    x.Title,
                    x.Body,
                    x.Status.ToString(),
                    x.ImageUrl,
                    x.CreatedAtUtc,
                    x.PublishedAtUtc,
                    Convert.ToBase64String(x.RowVersion)))
                .ToList();

            return new PagedResponse<ContentSummaryResponse>(
                items,
                content.Page,
                content.PageSize,
                content.TotalCount,
                content.TotalPages);
        }
    }
}
