using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;

using Template.Core.App.Users.DataTransfer;
using Template.Core.App.Users.Interfaces.Service;
using Template.Core.CrossCutting.Pagination;

namespace Template.Core.Api.Users;

/// <summary>
/// Administrative user management. Requires an authenticated (ADMIN) user.
/// </summary>
/// <param name="userAppService">Application service responsible for user business rules.</param>
[ApiController]
[Route("api/[controller]")]
public class UserController(IUserAppService userAppService) : ControllerBase
{
    /// <summary>
    /// Registers a new administrative user.
    /// </summary>
    /// <response code="200">Returns the created user.</response>
    /// <response code="401">No valid token.</response>
    /// <response code="422">Login already registered.</response>
    [HttpPost]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        return Ok(await userAppService.RegisterAsync(request, cancellationToken));
    }

    [HttpGet]
    [ProducesResponseType<PaginatedResponse<UserResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListAsync(
        [FromQuery] ListUsersRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await userAppService.ListAsync(request, cancellationToken));
    }

    /// <summary>Activates a user.</summary>
    [HttpPut("{id:int}/activate")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken = default)
        => Ok(await userAppService.ChangeStatusAsync(id, true, GetActorId(), cancellationToken));

    /// <summary>
    /// Deactivates a user. Since session revocation isn't part of this hotfix, tokens
    /// already issued keep expiring normally.
    /// </summary>
    [HttpPut("{id:int}/deactivate")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken = default)
        => Ok(await userAppService.ChangeStatusAsync(id, false, GetActorId(), cancellationToken));

    private int GetActorId()
    {
        string? sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return int.TryParse(sub, out int id)
            ? id
            : throw new UnauthorizedAccessException("User not identified in the token.");
    }
}
