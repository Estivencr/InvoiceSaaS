using InvoiceSaaS.Application.Common;
using InvoiceSaaS.Application.DTOs.User;
using InvoiceSaaS.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceSaaS.Api.Controllers;

[ApiController]
[Route("api/security")]
[Authorize(Roles = "Admin")]
public class SecurityController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUser;

    public SecurityController(IUserService userService, ICurrentUserService currentUser)
    {
        _userService = userService;
        _currentUser = currentUser;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var result = await _userService.GetAllAsync(_currentUser.CompanyId, pagination, ct);
        return Ok(ApiResponse<PagedResult<UserResponse>>.Ok(result));
    }

    [HttpGet("users/{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id, CancellationToken ct)
    {
        var result = await _userService.GetByIdAsync(_currentUser.CompanyId, id, ct);
        return Ok(ApiResponse<UserResponse>.Ok(result));
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken ct)
    {
        var result = await _userService.CreateAsync(_currentUser.CompanyId, request, ct);
        return Created(string.Empty, ApiResponse<UserResponse>.Ok(result, "User created."));
    }

    [HttpPut("users/{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request, CancellationToken ct)
    {
        var result = await _userService.UpdateAsync(_currentUser.CompanyId, id, request, ct);
        return Ok(ApiResponse<UserResponse>.Ok(result));
    }

    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
    {
        await _userService.DeleteAsync(_currentUser.CompanyId, id, ct);
        return Ok(ApiResponse.Ok("User deleted."));
    }

    [HttpPatch("users/{id:guid}/change-role")]
    public async Task<IActionResult> ChangeRole(Guid id, [FromBody] ChangeRoleRequest request, CancellationToken ct)
    {
        var result = await _userService.ChangeRoleAsync(_currentUser.CompanyId, id, request, ct);
        return Ok(ApiResponse<UserResponse>.Ok(result));
    }

    [HttpPatch("users/{id:guid}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(Guid id, CancellationToken ct)
    {
        var result = await _userService.ToggleStatusAsync(_currentUser.CompanyId, id, ct);
        return Ok(ApiResponse<UserResponse>.Ok(result));
    }
}
