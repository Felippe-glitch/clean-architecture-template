using Template.Core.CrossCutting.Pagination;
using Template.Core.Domain.Users.Entity;

namespace Template.Core.App.Users.DataTransfer;

public class ListUsersRequest : PaginatedRequest<User>
{
    public bool? Active { get; set; }
}
