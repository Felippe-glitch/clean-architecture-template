using Mapster;
using Microsoft.Extensions.Logging;

using Template.Core.App.Common;
using Template.Core.App.Users.DataTransfer;
using Template.Core.CrossCutting.Pagination;
using Template.Core.CrossCutting.Security;
using Template.Core.Domain.Users.Command;
using Template.Core.Domain.Users.Entity;
using Template.Core.Domain.Users.Interfaces.Service;
using Template.Core.Domain.Users.Repository;
using System.Threading.Tasks;
using System.Threading;
using Template.Core.Domain.Users.Repository.Filters;
using System.Collections.Generic;
using Template.Core.App.Users.Interfaces.Service;

namespace Template.Core.App.Users.Service;

public class UserAppService(
    IUserService userService,
    IPasswordHasher passwordHasher,
    IUserRepository userRepository,
    ILogger<UserAppService> logger,
    IUnitOfWork unitOfWork) : IUserAppService
{
    public async Task<UserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            RegisterUserCommand command = new()
            {
                Login = request.Login,
                PasswordHash = passwordHasher.Hash(request.Password),
                Email = request.Email,
                Role = request.Role!.Value,
            };

            User created = await userService.Register(command, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);

            return created.Adapt<UserResponse>();
        }
        catch
        {
            logger.LogError("Error registering user with login {Login}", request.Login);
            throw;
        }
    }

    public async Task<UserResponse> ChangeStatusAsync(
        int id,
        bool active,
        int actorId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            User user = await userService.ChangeStatus(id, active, actorId, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
            return user.Adapt<UserResponse>();
        }
        catch
        {
            logger.LogError("Error changing status of user with id {Id}", id);
            throw;
        }
    }

    public async Task<ListUsersResponse> ListAsync(ListUsersRequest request, CancellationToken cancellationToken)
    {
        ListUsersFilter filters = new()
        {
            Active = request.Active
        };

        PaginatedResult<User> result = await userRepository.FilterAsync(
            filters,
            request.Page,
            request.PageSize,
            request.SortBy,
            request.SortDirection,
            cancellationToken);

        return new ListUsersResponse
        {
            Data = result.Data.Adapt<List<UserResponse>>(),
            Page = result.PageNumber,
            PageSize = result.PageSize,
            Total = result.TotalItems,
            TotalPages = result.TotalPages,
        };
    }
}
