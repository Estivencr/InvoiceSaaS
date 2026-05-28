using InvoiceSaaS.Application.Common;
using InvoiceSaaS.Application.DTOs.Employee;
using InvoiceSaaS.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceSaaS.Api.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly ICurrentUserService _currentUser;

    public EmployeesController(IEmployeeService employeeService, ICurrentUserService currentUser)
    {
        _employeeService = employeeService;
        _currentUser = currentUser;
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
    {
        var result = await _employeeService.GetRolesAsync(_currentUser.CompanyId, ct);
        return Ok(ApiResponse<IEnumerable<RoleResponse>>.Ok(result));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var result = await _employeeService.GetAllAsync(_currentUser.CompanyId, pagination, ct);
        return Ok(ApiResponse<PagedResult<EmployeeResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _employeeService.GetByIdAsync(_currentUser.CompanyId, id, ct);
        return Ok(ApiResponse<EmployeeResponse>.Ok(result));
    }

    [HttpGet("by-role/{roleId:guid}")]
    public async Task<IActionResult> GetByRole(Guid roleId, CancellationToken ct)
    {
        var result = await _employeeService.GetByRoleAsync(_currentUser.CompanyId, roleId, ct);
        return Ok(ApiResponse<IEnumerable<EmployeeResponse>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateEmployeeRequest request, CancellationToken ct)
    {
        var result = await _employeeService.CreateAsync(_currentUser.CompanyId, request, ct);
        return Created(string.Empty, ApiResponse<EmployeeResponse>.Ok(result, "Employee created."));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeRequest request, CancellationToken ct)
    {
        var result = await _employeeService.UpdateAsync(_currentUser.CompanyId, id, request, ct);
        return Ok(ApiResponse<EmployeeResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _employeeService.DeleteAsync(_currentUser.CompanyId, id, ct);
        return Ok(ApiResponse.Ok("Employee deleted."));
    }
}
