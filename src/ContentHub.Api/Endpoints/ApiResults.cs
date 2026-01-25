using Microsoft.AspNetCore.Http;

namespace ContentHub.Api.Endpoints
{
    internal static class ApiResults
    {
        public static IResult ValidationProblem(string message)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["error"] = new[] { message }
            });
        }
    }
}
