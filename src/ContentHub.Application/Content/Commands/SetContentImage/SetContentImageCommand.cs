namespace ContentHub.Application.Content.Commands.SetContentImage
{
    public record SetContentImageCommand(
        Guid ContentId,
        string ImageUrl,
        string RowVersion);
}
