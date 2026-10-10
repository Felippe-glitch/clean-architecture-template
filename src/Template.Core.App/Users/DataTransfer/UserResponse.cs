using Template.Core.CrossCutting.Pagination;
using Template.Core.Domain.Users.Enums;

namespace Template.Core.App.Users.DataTransfer;

public record UserResponse
{
    public int Id { get; init; }
    public string Login { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public bool Active { get; init; }
}

public record ListUsersResponse : PaginatedResponse<UserResponse>
{
}