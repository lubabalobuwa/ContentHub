using ContentHub.Application.Content.Commands.ArchiveContent;
using ContentHub.Application.Content.Commands.CreateContent;
using ContentHub.Application.Content.Commands.DeleteContent;
using ContentHub.Application.Content.Commands.PublishContent;
using ContentHub.Application.Content.Commands.RestoreContent;
using ContentHub.Application.Content.Commands.SetContentImage;
using ContentHub.Application.Content.Commands.UpdateContent;
using ContentHub.Application.Content.Queries.GetArchivedContent;
using ContentHub.Application.Content.Queries.GetArchivedContentByAuthor;
using ContentHub.Application.Content.Queries.GetContentById;
using ContentHub.Application.Content.Queries.GetDraftContent;
using ContentHub.Application.Content.Queries.GetDraftContentByAuthor;
using ContentHub.Application.Content.Queries.GetPublishedContentByAuthor;
using ContentHub.Application.Content.Queries.GetPublishedContent;
using ContentHub.Application.Common.Interfaces;
using ContentHub.Application.Users.Commands.AuthenticateUser;
using ContentHub.Application.Users.Commands.CreateUser;
using ContentHub.Application.Users.Commands.ForgotPassword;
using ContentHub.Application.Users.Commands.RefreshToken;
using ContentHub.Application.Users.Commands.ResetPassword;
using ContentHub.Application.Users.Commands.ResetPasswordWithToken;
using ContentHub.Application.Users.Commands.ResendVerification;
using ContentHub.Application.Users.Commands.SetUserProfileImage;
using ContentHub.Application.Users.Commands.SetUserStatus;
using ContentHub.Application.Users.Commands.VerifyEmail;
using ContentHub.Application.Users.Queries.GetUsers;
using ContentHub.Application.Users.Queries.GetUserProfile;
using Microsoft.Extensions.DependencyInjection;
using ContentHub.Application.Users.ExternalLogin;

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
            services.AddScoped<GetPublishedContentByAuthorHandler>();
            services.AddScoped<PublishContentHandler>();
            services.AddScoped<RestoreContentHandler>();
            services.AddScoped<SetContentImageHandler>();
            services.AddScoped<UpdateContentHandler>();
            
            services.AddScoped<IValidator<CreateContentCommand>, CreateContentValidator>();
            services.AddScoped<IValidator<UpdateContentCommand>, UpdateContentValidator>();
            services.AddScoped<IValidator<ArchiveContentCommand>, ArchiveContentValidator>();
            services.AddScoped<IValidator<DeleteContentCommand>, DeleteContentValidator>();
            services.AddScoped<IValidator<RestoreContentCommand>, RestoreContentValidator>();
            services.AddScoped<IValidator<PublishContentCommand>, PublishContentValidator>();
            services.AddScoped<IValidator<SetContentImageCommand>, SetContentImageValidator>();
            services.AddScoped<IValidator<ExternalLoginCommand>, ExternalLoginValidator>();

            services.AddScoped<AuthenticateUserHandler>();
            services.AddScoped<CreateUserHandler>();
            services.AddScoped<ForgotPasswordHandler>();
            services.AddScoped<RefreshTokenHandler>();
            services.AddScoped<ResetPasswordHandler>();
            services.AddScoped<ResetPasswordWithTokenHandler>();
            services.AddScoped<ResendVerificationHandler>();
            services.AddScoped<SetUserProfileImageHandler>();
            services.AddScoped<SetUserStatusHandler>();
            services.AddScoped<VerifyEmailHandler>();
            services.AddScoped<ExternalLoginHandler>();
            services.AddScoped<GetUsersHandler>();
            services.AddScoped<GetUserProfileHandler>();

            services.AddScoped<IValidator<AuthenticateUserCommand>, AuthenticateUserValidator>();
            services.AddScoped<IValidator<CreateUserCommand>, CreateUserValidator>();
            services.AddScoped<IValidator<ResetPasswordCommand>, ResetPasswordValidator>();
            services.AddScoped<IValidator<ResetPasswordWithTokenCommand>, ResetPasswordWithTokenValidator>();

            return services;
        }
    }
}
