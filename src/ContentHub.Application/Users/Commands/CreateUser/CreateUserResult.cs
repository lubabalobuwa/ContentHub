namespace ContentHub.Application.Users.Commands.CreateUser
{
    public record CreateUserResult(Guid UserId, bool EmailSent, bool EmailSendFailed);
}
