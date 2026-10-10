using Template.Core.Domain.Users.Entity;

namespace Template.Core.Domain.Users.Repository.Filters;

public class ListUsersFilter
{
    public bool? Active { get; set; }
}
