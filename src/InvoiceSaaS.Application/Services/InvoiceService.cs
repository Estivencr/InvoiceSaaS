using AutoMapper;
using InvoiceSaaS.Application.Common;
using InvoiceSaaS.Application.DTOs.Invoice;
using InvoiceSaaS.Domain.Entities;
using InvoiceSaaS.Domain.Enums;
using InvoiceSaaS.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InvoiceSaaS.Application.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly ILogger<InvoiceService> _logger;

    public InvoiceService(IUnitOfWork uow, IMapper mapper, ILogger<InvoiceService> logger)
    {
        _uow = uow;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<InvoiceResponse>> GetAllAsync(Guid companyId, PaginationParams p, CancellationToken ct = default)
    {
        var query = _uow.Repository<Invoice>().Query()
            .Include(i => i.Customer)
            .Include(i => i.CreatedBy)
            .Include(i => i.Details)
            .Where(i => i.CompanyId == companyId && !i.IsDeleted);

        if (!string.IsNullOrWhiteSpace(p.Status) && Enum.TryParse<InvoiceStatus>(p.Status, true, out var status))
            query = query.Where(i => i.Status == status);

        if (p.DateFrom.HasValue)
            query = query.Where(i => i.IssueDate >= p.DateFrom.Value);

        if (p.DateTo.HasValue)
            query = query.Where(i => i.IssueDate <= p.DateTo.Value);

        if (!string.IsNullOrWhiteSpace(p.Search))
            query = query.Where(i => i.InvoiceNumber.Contains(p.Search) || i.Customer.Name.Contains(p.Search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((p.Page - 1) * p.PageSize)
            .Take(p.PageSize)
            .ToListAsync(ct);

        return PagedResult<InvoiceResponse>.Create(_mapper.Map<IEnumerable<InvoiceResponse>>(items), total, p.Page, p.PageSize);
    }

    public async Task<InvoiceResponse> GetByIdAsync(Guid companyId, Guid id, CancellationToken ct = default)
    {
        var invoice = await _uow.Repository<Invoice>().Query()
            .Include(i => i.Customer)
            .Include(i => i.CreatedBy)
            .Include(i => i.Details.OrderBy(d => d.Sequence))
            .FirstOrDefaultAsync(i => i.Id == id && i.CompanyId == companyId && !i.IsDeleted, ct)
            ?? throw new NotFoundException("Invoice", id);

        return _mapper.Map<InvoiceResponse>(invoice);
    }

    public async Task<InvoiceResponse> CreateAsync(Guid companyId, Guid userId, CreateInvoiceRequest request, CancellationToken ct = default)
    {
        var customer = await _uow.Repository<Customer>()
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId && c.CompanyId == companyId && !c.IsDeleted, ct)
            ?? throw new NotFoundException("Customer", request.CustomerId);

        var invoiceNumber = await GenerateInvoiceNumberAsync(companyId, ct);

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            InvoiceNumber = invoiceNumber,
            CustomerId = request.CustomerId,
            CreatedById = userId,
            IssueDate = request.IssueDate,
            DueDate = request.DueDate,
            TaxRate = request.TaxRate,
            Notes = request.Notes,
            Status = InvoiceStatus.Pending
        };

        int seq = 1;
        foreach (var detail in request.Details)
        {
            var d = _mapper.Map<InvoiceDetail>(detail);
            d.Id = Guid.NewGuid();
            d.InvoiceId = invoice.Id;
            d.Sequence = seq++;
            d.CalculateAmount();
            invoice.Details.Add(d);
        }

        invoice.RecalculateTotals();

        await _uow.Repository<Invoice>().AddAsync(invoice, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Invoice {Number} created for company {CompanyId}", invoiceNumber, companyId);

        invoice.Customer = customer;
        return _mapper.Map<InvoiceResponse>(invoice);
    }

    public async Task<InvoiceResponse> UpdateAsync(Guid companyId, Guid id, UpdateInvoiceRequest request, CancellationToken ct = default)
    {
        var invoice = await _uow.Repository<Invoice>().Query()
            .Include(i => i.Customer)
            .Include(i => i.CreatedBy)
            .Include(i => i.Details)
            .FirstOrDefaultAsync(i => i.Id == id && i.CompanyId == companyId && !i.IsDeleted, ct)
            ?? throw new NotFoundException("Invoice", id);

        if (invoice.Status == InvoiceStatus.Paid)
            throw new BusinessException("Cannot edit a paid invoice.");

        if (invoice.Status == InvoiceStatus.Cancelled)
            throw new BusinessException("Cannot edit a cancelled invoice.");

        var customer = await _uow.Repository<Customer>()
            .FirstOrDefaultAsync(c => c.Id == request.CustomerId && c.CompanyId == companyId && !c.IsDeleted, ct)
            ?? throw new NotFoundException("Customer", request.CustomerId);

        invoice.CustomerId = request.CustomerId;
        invoice.IssueDate = request.IssueDate;
        invoice.DueDate = request.DueDate;
        invoice.TaxRate = request.TaxRate;
        invoice.Notes = request.Notes;
        invoice.UpdatedAt = DateTime.UtcNow;
        invoice.Details.Clear();

        int seq = 1;
        foreach (var detail in request.Details)
        {
            var d = _mapper.Map<InvoiceDetail>(detail);
            d.Id = Guid.NewGuid();
            d.InvoiceId = invoice.Id;
            d.Sequence = seq++;
            d.CalculateAmount();
            invoice.Details.Add(d);
        }

        invoice.RecalculateTotals();
        _uow.Repository<Invoice>().Update(invoice);
        await _uow.SaveChangesAsync(ct);

        invoice.Customer = customer;
        return _mapper.Map<InvoiceResponse>(invoice);
    }

    public async Task DeleteAsync(Guid companyId, Guid id, CancellationToken ct = default)
    {
        var invoice = await _uow.Repository<Invoice>()
            .FirstOrDefaultAsync(i => i.Id == id && i.CompanyId == companyId && !i.IsDeleted, ct)
            ?? throw new NotFoundException("Invoice", id);

        if (invoice.Status == InvoiceStatus.Paid)
            throw new BusinessException("Cannot delete a paid invoice.");

        invoice.IsDeleted = true;
        invoice.UpdatedAt = DateTime.UtcNow;
        _uow.Repository<Invoice>().Update(invoice);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<InvoiceResponse> UpdateStatusAsync(Guid companyId, Guid id, UpdateInvoiceStatusRequest request, CancellationToken ct = default)
    {
        var invoice = await _uow.Repository<Invoice>().Query()
            .Include(i => i.Customer)
            .Include(i => i.CreatedBy)
            .Include(i => i.Details)
            .FirstOrDefaultAsync(i => i.Id == id && i.CompanyId == companyId && !i.IsDeleted, ct)
            ?? throw new NotFoundException("Invoice", id);

        ValidateStatusTransition(invoice.Status, request.Status);

        invoice.Status = request.Status;
        if (request.Status == InvoiceStatus.Paid)
            invoice.PaymentDate = request.PaymentDate ?? DateTime.UtcNow;

        invoice.UpdatedAt = DateTime.UtcNow;
        _uow.Repository<Invoice>().Update(invoice);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<InvoiceResponse>(invoice);
    }

    public async Task<InvoiceResponse> AddDetailAsync(Guid companyId, Guid invoiceId, InvoiceDetailRequest request, CancellationToken ct = default)
    {
        var invoice = await _uow.Repository<Invoice>().Query()
            .Include(i => i.Customer)
            .Include(i => i.CreatedBy)
            .Include(i => i.Details)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.CompanyId == companyId && !i.IsDeleted, ct)
            ?? throw new NotFoundException("Invoice", invoiceId);

        if (invoice.Status != InvoiceStatus.Pending)
            throw new BusinessException("Can only add details to pending invoices.");

        var detail = _mapper.Map<InvoiceDetail>(request);
        detail.Id = Guid.NewGuid();
        detail.InvoiceId = invoiceId;
        detail.Sequence = invoice.Details.Count + 1;
        detail.CalculateAmount();
        invoice.Details.Add(detail);
        invoice.RecalculateTotals();
        invoice.UpdatedAt = DateTime.UtcNow;

        _uow.Repository<Invoice>().Update(invoice);
        await _uow.SaveChangesAsync(ct);

        return _mapper.Map<InvoiceResponse>(invoice);
    }

    public async Task RemoveDetailAsync(Guid companyId, Guid invoiceId, Guid detailId, CancellationToken ct = default)
    {
        var invoice = await _uow.Repository<Invoice>().Query()
            .Include(i => i.Details)
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.CompanyId == companyId && !i.IsDeleted, ct)
            ?? throw new NotFoundException("Invoice", invoiceId);

        if (invoice.Status != InvoiceStatus.Pending)
            throw new BusinessException("Can only remove details from pending invoices.");

        var detail = invoice.Details.FirstOrDefault(d => d.Id == detailId)
            ?? throw new NotFoundException("InvoiceDetail", detailId);

        if (invoice.Details.Count == 1)
            throw new BusinessException("Invoice must have at least one detail.");

        invoice.Details.Remove(detail);
        invoice.RecalculateTotals();
        invoice.UpdatedAt = DateTime.UtcNow;

        _uow.Repository<Invoice>().Update(invoice);
        await _uow.SaveChangesAsync(ct);
    }

    private static void ValidateStatusTransition(InvoiceStatus current, InvoiceStatus next)
    {
        var allowed = (current, next) switch
        {
            (InvoiceStatus.Pending, InvoiceStatus.Paid) => true,
            (InvoiceStatus.Pending, InvoiceStatus.Cancelled) => true,
            _ => false
        };

        if (!allowed)
            throw new BusinessException($"Invalid status transition from '{current}' to '{next}'.");
    }

    private async Task<string> GenerateInvoiceNumberAsync(Guid companyId, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var prefix = $"INV-{now:yyyy-MM}-";
        var count = await _uow.Repository<Invoice>()
            .CountAsync(i => i.CompanyId == companyId && i.InvoiceNumber.StartsWith(prefix), ct);
        return $"{prefix}{(count + 1):D5}";
    }
}
