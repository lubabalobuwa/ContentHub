using ContentHub.Application.Common.Interfaces;
using ContentHub.Application.Messaging;
using ContentHub.Infrastructure.Authentication;
using ContentHub.Infrastructure.Messaging;
using ContentHub.Infrastructure.Persistence;
using ContentHub.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ContentHub.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ContentHubDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<IContentRepository, ContentRepository>();
            services.AddScoped<IContentReadRepository, ContentReadRepository>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IRabbitMqPublisher, RabbitMqPublisher>();

            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

            return services;
        }
    }
}
