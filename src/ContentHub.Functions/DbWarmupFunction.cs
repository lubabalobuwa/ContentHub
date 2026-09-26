using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ContentHub.Functions;

public class DbWarmupFunction
{
    private readonly HttpClient _httpClient;

    public DbWarmupFunction(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    [Function("DbWarmup")]
    public async Task Run([TimerTrigger("0 45 6 * * *")] TimerInfo timer, FunctionContext context)
    {
        var logger = context.GetLogger("DbWarmup");

        try
        {
            var apiBaseUrl = Environment.GetEnvironmentVariable("ApiBaseUrl")
                ?? "https://yourapi.azurewebsites.net";

            var response = await _httpClient.GetAsync($"{apiBaseUrl}/health/db");
            logger.LogInformation("Warmup ping returned {StatusCode}", response.StatusCode);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Warmup ping failed");
        }
    }
}
