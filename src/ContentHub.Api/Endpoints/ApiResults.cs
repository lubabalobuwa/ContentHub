using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ContentHub.Api.Endpoints
{
    internal static class ApiResults
    {
        public static IResult Unauthorized(string? detail = null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Unauthorized",
                detail: detail);
        }

        public static IResult Forbidden(string? detail = null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Forbidden",
                detail: detail);
        }

        public static IResult NotFound(string? detail = null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                detail: detail);
        }

        public static IResult Conflict(string? detail = null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict",
                detail: detail);
        }

        public static IResult ValidationProblem(string message)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["error"] = new[] { message }
            });
        }

        public static IResult Unexpected(string? detail = null)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Unexpected error",
                detail: detail);
        }
    }
}
