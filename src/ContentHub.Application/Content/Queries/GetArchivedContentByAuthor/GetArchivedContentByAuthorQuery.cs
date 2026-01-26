using System;

namespace ContentHub.Application.Content.Queries.GetArchivedContentByAuthor
{
    public record GetArchivedContentByAuthorQuery(Guid AuthorId, int Page, int PageSize);
}
