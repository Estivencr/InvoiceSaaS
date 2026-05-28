using InvoiceSaaS.Application.Common;
using InvoiceSaaS.Application.DTOs.Auth;
using InvoiceSaaS.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceSaaS.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;

    public AuthController(IAuthService authService, ICurrentUserService currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 201)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(request, ct);
        return Created(string.Empty, ApiResponse<LoginResponse>.Ok(result, "Company registered successfully."));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(request, ct);
        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        var result = await _authService.RefreshTokenAsync(request, ct);
        return Ok(ApiResponse<LoginResponse>.Ok(result));
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        await _authService.LogoutAsync(_currentUser.UserId, ct);
        return Ok(ApiResponse.Ok("Logged out successfully."));
    }

    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        await _authService.ChangePasswordAsync(_currentUser.UserId, request, ct);
        return Ok(ApiResponse.Ok("Password changed successfully."));
    }
}
