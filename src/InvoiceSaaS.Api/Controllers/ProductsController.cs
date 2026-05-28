using InvoiceSaaS.Application.Common;
using InvoiceSaaS.Application.DTOs.Product;
using InvoiceSaaS.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceSaaS.Api.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ICurrentUserService _currentUser;

    public ProductsController(IProductService productService, ICurrentUserService currentUser)
    {
        _productService = productService;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination, CancellationToken ct)
    {
        var result = await _productService.GetAllAsync(_currentUser.CompanyId, pagination, ct);
        return Ok(ApiResponse<PagedResult<ProductResponse>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _productService.GetByIdAsync(_currentUser.CompanyId, id, ct);
        return Ok(ApiResponse<ProductResponse>.Ok(result));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string term, CancellationToken ct)
    {
        var result = await _productService.SearchAsync(_currentUser.CompanyId, term, ct);
        return Ok(ApiResponse<IEnumerable<ProductResponse>>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var result = await _productService.CreateAsync(_currentUser.CompanyId, request, ct);
        return Created(string.Empty, ApiResponse<ProductResponse>.Ok(result, "Product created."));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var result = await _productService.UpdateAsync(_currentUser.CompanyId, id, request, ct);
        return Ok(ApiResponse<ProductResponse>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _productService.DeleteAsync(_currentUser.CompanyId, id, ct);
        return Ok(ApiResponse.Ok("Product deleted."));
    }
}
