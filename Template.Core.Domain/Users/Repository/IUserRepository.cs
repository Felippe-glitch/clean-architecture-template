using Template.Core.CrossCutting.Pagination;
using Template.Core.Domain.Users.Entity;
using Template.Core.Domain.Users.Repository.Filters;

namespace Template.Core.Domain.Users.Repository;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User> GetByLoginAsync(string login, CancellationToken cancellationToken);
    Task<bool> HasAnyAsync(CancellationToken cancellationToken);

    Task<PaginatedResult<User>> FilterAsync(ListUsersFilter filters, CancellationToken cancellationToken)
        => FilterAsync(filters, page: 1, pageSize: 10, sortBy: "Id", sortDirection: SortDirection.Ascending, cancellationToken);

    Task<PaginatedResult<User>> FilterAsync(
        ListUsersFilter filters,
        int page,
        int pageSize,
        string sortBy,
        SortDirection sortDirection,
        CancellationToken cancellationToken);
}
