using ContentHub.Application.Content.Commands.ArchiveContent;
using ContentHub.Application.Content.Commands.CreateContent;
using ContentHub.Application.Content.Commands.DeleteContent;
using ContentHub.Application.Content.Commands.PublishContent;
using ContentHub.Application.Content.Commands.RestoreContent;
using ContentHub.Application.Content.Commands.UpdateContent;
using ContentHub.Application.Content.Queries.GetArchivedContent;
using ContentHub.Application.Content.Queries.GetArchivedContentByAuthor;
using ContentHub.Application.Content.Queries.GetContentById;
using ContentHub.Application.Content.Queries.GetDraftContent;
using ContentHub.Application.Content.Queries.GetDraftContentByAuthor;
using ContentHub.Application.Content.Queries.GetPublishedContent;
using ContentHub.Application.Users.Commands.CreateUser;
using Microsoft.Extensions.DependencyInjection;

namespace ContentHub.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<ArchiveContentHandler>();
            services.AddScoped<CreateContentHandler>();
            services.AddScoped<DeleteContentHandler>();
            services.AddScoped<GetArchivedContentHandler>();
            services.AddScoped<GetArchivedContentByAuthorHandler>();
            services.AddScoped<GetContentByIdHandler>();
            services.AddScoped<GetDraftContentHandler>();
            services.AddScoped<GetDraftContentByAuthorHandler>();
            services.AddScoped<GetPublishedContentHandler>();
            services.AddScoped<PublishContentHandler>();
            services.AddScoped<RestoreContentHandler>();
            services.AddScoped<UpdateContentHandler>();
            
            services.AddScoped<CreateUserHandler>();

            return services;
        }
    }
}
