using System.Collections.Generic;

namespace Template.Core.CrossCutting.Pagination;

public record PaginatedResponse<T>
{
    public IReadOnlyList<T> Data { get; init; } = [];
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int Total { get; init; }
    public int TotalPages { get; init; }
}
