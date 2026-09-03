using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using Template.Core.Api.Config;
using Template.Core.App.Auth.DataTransfer;
using Template.Core.App.Auth.Service;

namespace Template.Core.Api.Auth;

/// <summary>
/// Authentication: issues JWT tokens for administrative access.
/// </summary>
/// <param name="authAppService">Application service responsible for authentication.</param>
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting(RateLimitConfig.AuthPolicyName)]
public class AuthController(IAuthAppService authAppService) : ControllerBase
{
    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST /api/Auth/login
    ///     { "login": "admin", "password": "..." }
    ///
    /// Use the token in the <c>Authorization: Bearer &lt;token&gt;</c> header on protected routes.
    /// </remarks>
    /// <response code="200">Returns the token and the user data.</response>
    /// <response code="401">Invalid login or password.</response>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken = default)
    {
        return Ok(await authAppService.LoginAsync(request, cancellationToken));
    }

    /// <summary>
    /// Renews the session from a valid refresh token, issuing a new access/refresh pair.
    /// </summary>
    /// <remarks>
    /// Refresh is <b>stateless</b> (ADR 08): the previous refresh token is <b>not</b> rotated or
    /// revoked and remains valid until it expires. There is no server-side logout — session
    /// revocation is finding F4 from the July/2026 audit report, planned for v0.0.2 (ATOS-81).
    /// Today, the only way to invalidate sessions is to rotate the signing key, which brings
    /// down every session at once.
    /// </remarks>
    /// <response code="200">New access token and refresh token.</response>
    /// <response code="401">Invalid or expired refresh token.</response>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken = default)
    {
        return Ok(await authAppService.RefreshAsync(request.RefreshToken, cancellationToken));
    }
}
