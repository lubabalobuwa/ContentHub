using ContentHub.Api.Contracts.Requests;
using ContentHub.Api.Contracts.Responses;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace ContentHub.Api.IntegrationTests
{
    public class AuthEndpointsTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public AuthEndpointsTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Login_ReturnsToken_ForValidCredentials()
        {
            var client = _factory.CreateClient();
            var auth = await RegisterAndLoginAsync(client);

            Assert.False(string.IsNullOrWhiteSpace(auth.Token));
            Assert.Equal("Author", auth.Role);
        }

        [Fact]
        public async Task CreateContent_ReturnsUnauthorized_WhenNoToken()
        {
            var client = _factory.CreateClient();
            var response = await client.PostAsJsonAsync("/api/content",
                new CreateContentRequest("Title", "Body"));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateContent_ReturnsCreated_WhenAuthenticated()
        {
            var client = _factory.CreateClient();
            var auth = await RegisterAndLoginAsync(client);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", auth.Token);

            var response = await client.PostAsJsonAsync("/api/content",
                new CreateContentRequest("Title", "Body"));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Drafts_ReturnsForbidden_ForNonAdmin()
        {
            var client = _factory.CreateClient();
            var auth = await RegisterAndLoginAsync(client);

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", auth.Token);

            var response = await client.GetAsync("/api/content/drafts");

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private static async Task<AuthResponse> RegisterAndLoginAsync(HttpClient client)
        {
            var email = $"{Guid.NewGuid()}@test.local";
            const string password = "Passw0rd!";

            var createResponse = await client.PostAsJsonAsync("/api/users",
                new CreateUserRequest(email, "Test User", password));
            createResponse.EnsureSuccessStatusCode();

            var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
                new LoginRequest(email, password));
            loginResponse.EnsureSuccessStatusCode();

            var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
            return auth ?? throw new InvalidOperationException("Auth response was null.");
        }
    }
}
