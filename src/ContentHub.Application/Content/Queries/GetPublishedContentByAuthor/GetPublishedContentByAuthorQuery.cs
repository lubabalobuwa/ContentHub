using System;

namespace ContentHub.Application.Content.Queries.GetPublishedContentByAuthor
{
    public record GetPublishedContentByAuthorQuery(Guid AuthorId, int Page, int PageSize);
}
