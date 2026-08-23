using System.Collections.Generic;

namespace Template.Core.App.Common;

public record PaginatedResponse<T>
{
    public IReadOnlyList<T> Data { get; init; } = [];
    public int Page { get; init; }
    public int Qt { get; init; }
    public int Total { get; init; }
    public int TotalPages { get; init; }
}