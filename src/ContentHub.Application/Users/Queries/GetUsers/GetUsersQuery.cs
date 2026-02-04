namespace ContentHub.Application.Users.Queries.GetUsers
{
    public record GetUsersQuery(int Page, int PageSize, string? Search);
}
