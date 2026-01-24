namespace ContentHub.Api.Contracts.Requests
{
    public record CreateUserRequest(
        string Email, 
        string DisplayName
    );
}
