using ContentHub.Application.Content.Commands.CreateContent;
using ContentHub.Application.Content.Commands.PublishContent;
using ContentHub.Application.Content.Queries.GetContentById;
using ContentHub.Application.Content.Queries.GetPublishedContent;
using Microsoft.Extensions.DependencyInjection;

namespace ContentHub.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<CreateContentHandler>();
            services.AddScoped<GetContentByIdHandler>();
            services.AddScoped<GetPublishedContentHandler>();
            services.AddScoped<PublishContentHandler>();

            return services;
        }
    }
}
