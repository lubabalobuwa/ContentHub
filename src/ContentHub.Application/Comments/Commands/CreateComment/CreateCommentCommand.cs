namespace ContentHub.Application.Comments.Commands.CreateComment
{
    public record CreateCommentCommand(Guid ContentItemId, string Text);
}
