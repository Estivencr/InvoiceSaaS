using AutoMapper;
using InvoiceSaaS.Application.Common;
using InvoiceSaaS.Application.DTOs.Product;
using InvoiceSaaS.Domain.Entities;
using InvoiceSaaS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace InvoiceSaaS.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public ProductService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductResponse>> GetAllAsync(Guid companyId, PaginationParams p, CancellationToken ct = default)
    {
        var query = _uow.Repository<Product>().Query()
            .Where(x => x.CompanyId == companyId && !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(p.Search))
            query = query.Where(x => x.Name.Contains(p.Search) ||
                (x.SKU != null && x.SKU.Contains(p.Search)) ||
                (x.Category != null && x.Category.Contains(p.Search)));

        if (!string.IsNullOrWhiteSpace(p.Status))
        {
            var active = p.Status.Equals("active", StringComparison.OrdinalIgnoreCase);
            query = query.Where(x => x.IsActive == active);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(x => x.Name)
            .Skip((p.Page - 1) * p.PageSize)
            .Take(p.PageSize)
            .ToListAsync(ct);

        return PagedResult<ProductResponse>.Create(_mapper.Map<IEnumerable<ProductResponse>>(items), total, p.Page, p.PageSize);
    }

    public async Task<ProductResponse> GetByIdAsync(Guid companyId, Guid id, CancellationToken ct = default)
    {
        var product = await _uow.Repository<Product>()
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Product", id);

        return _mapper.Map<ProductResponse>(product);
    }

    public async Task<IEnumerable<ProductResponse>> SearchAsync(Guid companyId, string term, CancellationToken ct = default)
    {
        var products = await _uow.Repository<Product>()
            .FindAsync(x => x.CompanyId == companyId && !x.IsDeleted && x.IsActive &&
                (x.Name.Contains(term) || (x.SKU != null && x.SKU.Contains(term))), ct);

        return _mapper.Map<IEnumerable<ProductResponse>>(products);
    }

    public async Task<ProductResponse> CreateAsync(Guid companyId, CreateProductRequest request, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(request.SKU))
        {
            var existing = await _uow.Repository<Product>()
                .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.SKU == request.SKU && !x.IsDeleted, ct);
            if (existing != null)
                throw new BusinessException($"A product with SKU '{request.SKU}' already exists.");
        }

        var product = _mapper.Map<Product>(request);
        product.Id = Guid.NewGuid();
        product.CompanyId = companyId;

        await _uow.Repository<Product>().AddAsync(product, ct);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<ProductResponse>(product);
    }

    public async Task<ProductResponse> UpdateAsync(Guid companyId, Guid id, UpdateProductRequest request, CancellationToken ct = default)
    {
        var product = await _uow.Repository<Product>()
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Product", id);

        if (!string.IsNullOrWhiteSpace(request.SKU))
        {
            var skuTaken = await _uow.Repository<Product>()
                .FirstOrDefaultAsync(x => x.CompanyId == companyId && x.SKU == request.SKU && x.Id != id && !x.IsDeleted, ct);
            if (skuTaken != null)
                throw new BusinessException($"SKU '{request.SKU}' is already used by another product.");
        }

        _mapper.Map(request, product);
        product.UpdatedAt = DateTime.UtcNow;
        _uow.Repository<Product>().Update(product);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<ProductResponse>(product);
    }

    public async Task DeleteAsync(Guid companyId, Guid id, CancellationToken ct = default)
    {
        var product = await _uow.Repository<Product>()
            .FirstOrDefaultAsync(x => x.Id == id && x.CompanyId == companyId && !x.IsDeleted, ct)
            ?? throw new NotFoundException("Product", id);

        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;
        _uow.Repository<Product>().Update(product);
        await _uow.SaveChangesAsync(ct);
    }
}
