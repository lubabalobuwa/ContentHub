using ContentHub.Api.Contracts.Requests;
using ContentHub.Api.Contracts.Responses;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Application.Common.Options;
using ContentHub.Application.Content.Commands.SetContentImage;
using ContentHub.Application.Users.Commands.SetUserProfileImage;
using ContentHub.Domain.Content;
using ContentHub.Domain.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ContentHub.Api.Endpoints
{
    public static class UploadEndpoints
    {
        private const long MaxFileSizeBytes = 5 * 1024 * 1024;
        private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png"
        };

        public static IEndpointRouteBuilder MapUploadEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/uploads").WithTags("Uploads").RequireRateLimiting("general");

            group.MapPost("/content/{contentId:guid}/sas", async (
                Guid contentId,
                [FromBody] BlobUploadRequest request,
                [FromServices] IContentRepository contentRepository,
                [FromServices] ICurrentUserService currentUser,
                [FromServices] IBlobStorageService blobStorage,
                [FromServices] IOptions<BlobStorageSettings> options) =>
            {
                if (contentId == Guid.Empty)
                    return ApiResults.ValidationProblem("ContentId is required.");

                var content = await contentRepository.GetByIdAsync(contentId);
                if (content is null)
                    return ApiResults.NotFound("Content not found.");

                if (!currentUser.IsAuthenticated || currentUser.UserId is null)
                    return ApiResults.Unauthorized();

                if (currentUser.Role != UserRole.Admin && currentUser.UserId.Value != content.AuthorId)
                    return ApiResults.Forbidden();

                if (content.Status == ContentStatus.Archived)
                    return ApiResults.ValidationProblem("Archived content cannot be updated.");

                if (!TryValidateUploadRequest(request, out var extension, out var error))
                    return error!;

                var blobName = $"content/{contentId:D}/{Guid.NewGuid():N}.{extension}";
                var sas = await blobStorage.CreateUploadAsync(
                    blobName,
                    request.ContentType,
                    TimeSpan.FromMinutes(options.Value.SasExpiryMinutes),
                    CancellationToken.None);

                return Results.Ok(new BlobUploadResponse(sas.UploadUrl, sas.BlobUrl, sas.BlobName));
            }).RequireAuthorization();

            group.MapPost("/content/{contentId:guid}/complete", async (
                Guid contentId,
                [FromBody] CompleteContentImageUploadRequest request,
                [FromServices] IBlobStorageService blobStorage,
                [FromServices] SetContentImageHandler handler) =>
            {
                if (contentId == Guid.Empty)
                    return ApiResults.ValidationProblem("ContentId is required.");

                if (string.IsNullOrWhiteSpace(request.BlobName))
                    return ApiResults.ValidationProblem("BlobName is required.");

                if (!request.BlobName.StartsWith($"content/{contentId:D}/", StringComparison.OrdinalIgnoreCase))
                    return ApiResults.ValidationProblem("BlobName does not match content.");

                var properties = await blobStorage.GetPropertiesAsync(request.BlobName, CancellationToken.None);
                if (properties is null)
                    return ApiResults.NotFound("Uploaded blob not found.");

                if (properties.ContentLength > MaxFileSizeBytes)
                    return ApiResults.ValidationProblem("Image exceeds 5 MB limit.");

                if (string.IsNullOrWhiteSpace(properties.ContentType) ||
                    !AllowedContentTypes.Contains(properties.ContentType))
                    return ApiResults.ValidationProblem("Invalid image content type.");

                var result = await handler.HandleAsync(
                    new SetContentImageCommand(contentId, blobStorage.GetBlobUrl(request.BlobName), request.RowVersion));

                return result.IsSuccess
                    ? Results.Ok(new { message = "Content image updated." })
                    : MapFailure(result.Error);
            }).RequireAuthorization();

            group.MapPost("/profile/sas", async (
                [FromBody] BlobUploadRequest request,
                [FromServices] ICurrentUserService currentUser,
                [FromServices] IBlobStorageService blobStorage,
                [FromServices] IOptions<BlobStorageSettings> options) =>
            {
                if (!currentUser.IsAuthenticated || currentUser.UserId is null)
                    return ApiResults.Unauthorized();

                if (!TryValidateUploadRequest(request, out var extension, out var error))
                    return error!;

                var blobName = $"profiles/{currentUser.UserId.Value:D}/{Guid.NewGuid():N}.{extension}";
                var sas = await blobStorage.CreateUploadAsync(
                    blobName,
                    request.ContentType,
                    TimeSpan.FromMinutes(options.Value.SasExpiryMinutes),
                    CancellationToken.None);

                return Results.Ok(new BlobUploadResponse(sas.UploadUrl, sas.BlobUrl, sas.BlobName));
            }).RequireAuthorization();

            group.MapPost("/profile/complete", async (
                [FromBody] CompleteProfileImageUploadRequest request,
                [FromServices] ICurrentUserService currentUser,
                [FromServices] IBlobStorageService blobStorage,
                [FromServices] SetUserProfileImageHandler handler) =>
            {
                if (!currentUser.IsAuthenticated || currentUser.UserId is null)
                    return ApiResults.Unauthorized();

                if (string.IsNullOrWhiteSpace(request.BlobName))
                    return ApiResults.ValidationProblem("BlobName is required.");

                if (!request.BlobName.StartsWith($"profiles/{currentUser.UserId.Value:D}/", StringComparison.OrdinalIgnoreCase))
                    return ApiResults.ValidationProblem("BlobName does not match user.");

                var properties = await blobStorage.GetPropertiesAsync(request.BlobName, CancellationToken.None);
                if (properties is null)
                    return ApiResults.NotFound("Uploaded blob not found.");

                if (properties.ContentLength > MaxFileSizeBytes)
                    return ApiResults.ValidationProblem("Image exceeds 5 MB limit.");

                if (string.IsNullOrWhiteSpace(properties.ContentType) ||
                    !AllowedContentTypes.Contains(properties.ContentType))
                    return ApiResults.ValidationProblem("Invalid image content type.");

                var result = await handler.HandleAsync(
                    new SetUserProfileImageCommand(currentUser.UserId.Value, blobStorage.GetBlobUrl(request.BlobName)));

                return result.IsSuccess
                    ? Results.Ok(new { message = "Profile image updated." })
                    : MapFailure(result.Error);
            }).RequireAuthorization();

            return app;
        }

        private static bool TryValidateUploadRequest(
            BlobUploadRequest request,
            out string extension,
            out IResult? error)
        {
            extension = string.Empty;

            if (request.ContentLength <= 0 || request.ContentLength > MaxFileSizeBytes)
            {
                error = ApiResults.ValidationProblem("Image must be between 1 byte and 5 MB.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(request.ContentType) ||
                !AllowedContentTypes.Contains(request.ContentType))
            {
                error = ApiResults.ValidationProblem("Only JPG and PNG images are allowed.");
                return false;
            }

            extension = request.ContentType.Equals("image/png", StringComparison.OrdinalIgnoreCase)
                ? "png"
                : "jpg";

            error = null;
            return true;
        }

        private static IResult MapFailure(string error)
        {
            return error switch
            {
                "Unauthorized." => ApiResults.Unauthorized(),
                "Forbidden." => ApiResults.Forbidden(),
                "Content not found." => ApiResults.NotFound("Content not found."),
                "User not found." => ApiResults.NotFound("User not found."),
                _ => ApiResults.ValidationProblem(error)
            };
        }
    }
}
