namespace ContentHub.Application.Users.Commands.SetUserProfileImage
{
    public record SetUserProfileImageCommand(Guid UserId, string ImageUrl);
}
