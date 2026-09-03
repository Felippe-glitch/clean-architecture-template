using System.ComponentModel.DataAnnotations;

namespace Template.Core.CrossCutting.Pagination;

public class PaginatedRequest<T>
{
    [Range(1, 200, ErrorMessage = "pageSize must be between 1 and 200.")]
    public int PageSize { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "page must be greater than or equal to 1.")]
    public int Page { get; set; }
    public string SortBy { get; set; }
    public SortDirection SortDirection { get; set; }

    public PaginatedRequest(
        int pageSize = 10,
        int page = 1,
        string sortBy = "Id",
        SortDirection sortDirection = SortDirection.Ascending)
    {
        PageSize = pageSize;
        Page = page;
        SortBy = sortBy;
        SortDirection = sortDirection;
    }
}
