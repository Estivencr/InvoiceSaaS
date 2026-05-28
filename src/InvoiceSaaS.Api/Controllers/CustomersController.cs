using InvoiceSaaS.Application.Common;
using InvoiceSaaS.Application.DTOs.Customer;
using InvoiceSaaS.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceSaaS.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ICurrentUserService _currentUser;

    public CustomersController(ICustomerService customerService, ICurrentUserService currentUser)
    {
        _customerService = customerService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var result = await _customerService.GetAllAsync(_currentUser.CompanyId, pagination, ct);
        return Ok(ApiResponse<PagedResult<CustomerResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _customerService.GetByIdAsync(_currentUser.CompanyId, id, ct);
        return Ok(ApiResponse<CustomerResponse>.Ok(result));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string term, CancellationToken ct)
    {
        var result = await _customerService.SearchAsync(_currentUser.CompanyId, term, ct);
        return Ok(ApiResponse<IEnumerable<CustomerResponse>>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken ct)
    {
        var result = await _customerService.CreateAsync(_currentUser.CompanyId, _currentUser.UserId, request, ct);
        return Created(string.Empty, ApiResponse<CustomerResponse>.Ok(result, "Customer created."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken ct)
    {
        var result = await _customerService.UpdateAsync(_currentUser.CompanyId, id, request, ct);
        return Ok(ApiResponse<CustomerResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _customerService.DeleteAsync(_currentUser.CompanyId, id, ct);
        return Ok(ApiResponse.Ok("Customer deleted."));
    }
}
