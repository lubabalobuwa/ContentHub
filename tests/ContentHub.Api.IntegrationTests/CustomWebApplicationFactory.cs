using ContentHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ContentHub.Api.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        private const string JwtKey = "test-secret-key-for-integration-tests-should-be-longer-than-32-bytes";
        private const string JwtIssuer = "ContentHub";
        private const string JwtAudience = "ContentHub";
        private readonly SqliteConnection _connection = new("DataSource=:memory:");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Key"] = JwtKey,
                    ["Jwt:Issuer"] = JwtIssuer,
                    ["Jwt:Audience"] = JwtAudience,
                    ["Jwt:ExpiresMinutes"] = "60",
                    ["RabbitMq:ConnectionString"] = "amqp://guest:guest@localhost:5672",
                    ["ConnectionStrings:DefaultConnection"] = "DataSource=:memory:"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters.ValidIssuer = JwtIssuer;
                    options.TokenValidationParameters.ValidAudience = JwtAudience;
                    options.TokenValidationParameters.IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
                });

                services.RemoveAll<DbContextOptions<ContentHubDbContext>>();
                services.RemoveAll<ContentHubDbContext>();

                _connection.Open();
                services.AddDbContext<ContentHubDbContext>(options =>
                    options.UseSqlite(_connection));

                services.RemoveAll<IConnection>();
                services.AddSingleton(CreateMockConnection());

                using var scope = services.BuildServiceProvider().CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ContentHubDbContext>();
                db.Database.EnsureCreated();
            });
        }

        private static IConnection CreateMockConnection()
        {
            var connection = new Mock<IConnection>();
            var channel = new Mock<IChannel>();

            connection.Setup(c => c.CreateChannelAsync(
                    It.IsAny<CreateChannelOptions?>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(channel.Object);

            return connection.Object;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _connection.Dispose();
            }
        }
    }
}
