using System.Linq.Expressions;
using InvoiceSaaS.Domain.Interfaces;
using InvoiceSaaS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InvoiceSaaS.Infrastructure.Repositories;

public class GenericRepository<T> : IRepository<T> where T : class, IEntity
{
    protected readonly InvoiceSaasDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(InvoiceSaasDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _dbSet.FindAsync(new object[] { id }, ct);

    public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default) =>
        await _dbSet.ToListAsync(ct);

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await _dbSet.Where(predicate).ToListAsync(ct);

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await _dbSet.FirstOrDefaultAsync(predicate, ct);

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await _dbSet.AddAsync(entity, ct);

    public void Update(T entity) =>
        _dbSet.Update(entity);

    public void Remove(T entity) =>
        _dbSet.Remove(entity);

    public async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await _dbSet.CountAsync(predicate, ct);

    public IQueryable<T> Query() =>
        _dbSet.AsQueryable();
}
