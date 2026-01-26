using System.Collections.Generic;

namespace ContentHub.Api.Contracts.Responses
{
    public record PagedResponse<T>(
        IReadOnlyList<T> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages);
}
