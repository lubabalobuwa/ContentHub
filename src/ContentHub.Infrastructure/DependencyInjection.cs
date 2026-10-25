using ContentHub.Application.Common.Interfaces;
using ContentHub.Application.Common.Options;
using ContentHub.Application.Messaging;
using ContentHub.Infrastructure.Authentication;
using ContentHub.Infrastructure.Email;
using ContentHub.Infrastructure.Messaging;
using ContentHub.Infrastructure.Persistence;
using ContentHub.Infrastructure.Persistence.Repositories;
using ContentHub.Infrastructure.Rendering;
using ContentHub.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using System;

namespace ContentHub.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<MarkdownRenderService>();

            services.AddDbContext<ContentHubDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null)));

            var messagingEnabled = configuration.GetValue<bool>("Messaging:Enabled");
            if (messagingEnabled)
            {
                var fullyQualifiedNamespace = configuration["ServiceBus:FullyQualifiedNamespace"]
                    ?? throw new InvalidOperationException("ServiceBus:FullyQualifiedNamespace is required when messaging is enabled.");
                services.AddSingleton(new ServiceBusClient(fullyQualifiedNamespace, new DefaultAzureCredential()));
            }

            services.AddScoped<IContentRepository, ContentRepository>();
            services.AddScoped<IContentReadRepository, ContentReadRepository>();
            services.AddScoped<ICommentRepository, CommentRepository>();
            services.AddScoped<ICommentReadRepository, CommentReadRepository>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            if (messagingEnabled)
            {
                services.AddScoped<IMessagePublisher, ServiceBusMessagePublisher>();
                services.AddHostedService<ContentPublishedEmailWorker>();
            }
            else
            {
                services.AddScoped<IMessagePublisher, NoOpMessagePublisher>();
            }

            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
            services.Configure<AuthSettings>(configuration.GetSection("Auth"));
            services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));
            services.Configure<BlobStorageSettings>(configuration.GetSection("BlobStorage"));
            services.Configure<ServiceBusSettings>(configuration.GetSection("ServiceBus"));
            services.Configure<NewsletterSettings>(configuration.GetSection("Newsletter"));
            services.AddTransient<IEmailSender, SmtpEmailSender>();
            services.AddSingleton<IBlobStorageService, BlobStorageService>();

            return services;
        }
    }
}
