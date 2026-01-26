using System;

namespace ContentHub.Application.Content.Queries.GetDraftContentByAuthor
{
    public record GetDraftContentByAuthorQuery(Guid AuthorId, int Page, int PageSize);
}
