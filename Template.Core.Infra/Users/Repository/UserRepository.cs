using Microsoft.EntityFrameworkCore;

using Template.Core.CrossCutting.Pagination;
using Template.Core.Domain.Users.Entity;
using Template.Core.Domain.Users.Repository;
using Template.Core.Domain.Users.Repository.Filters;

namespace Template.Core.Infra.Users.Repository;

public class UserRepository(TemplateDbContext context) : GenericRepository<User>(context), IUserRepository
{
    public async Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken)
    {
        string normalized = User.NormalizeLogin(login);
        return await _context.Users
            .SingleOrDefaultAsync(u => u.Login == normalized, cancellationToken);
    }

    public async Task<bool> HasAnyAsync(CancellationToken cancellationToken)
        => await _context.Users.AnyAsync(cancellationToken);

    public async Task<PaginatedResult<User>> FilterAsync(
        ListUsersFilter filters,
        int page,
        int pageSize,
        string sortBy,
        SortDirection sortDirection,
        CancellationToken cancellationToken )
    {
        IQueryable<User> query = _context.Users;

        if (filters.Active.HasValue)
            query = query.Where(u => u.Active == filters.Active.Value);

        return await ListAsync(query, page, pageSize, sortBy, sortDirection);
    }
}
