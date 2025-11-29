using FreshMarket.Domain.Interfaces.Repositories;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories;

public class Repository<T>(FreshMarketDbContext context, ILogger<Repository<T>> logger)
    : IRepository<T> where T : class
{
    protected readonly DbSet<T> _set = context.Set<T>();
    protected readonly ILogger<Repository<T>> _logger = logger;

    public async Task<T?> GetByIdAsync(object id, CancellationToken ct = default)
    {
        Guard.AgainstNull(id, nameof(id));
        return await _set.FindAsync([id], ct);
    }

    public async Task<IReadOnlyList<T>> ListAllAsync(CancellationToken ct = default)
    {
        return await _set.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        var query = SpecificationEvaluator<T>.GetQuery(_set.AsQueryable(), spec);
        return await query.ToListAsync(ct);
    }

    public async Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        var query = SpecificationEvaluator<T>.GetQuery(_set.AsQueryable(), spec);
        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        Guard.AgainstNull(entity, nameof(entity));
        await _set.AddAsync(entity, ct);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        Guard.AgainstNullOrEmpty(entities, nameof(entities));
        await _set.AddRangeAsync(entities, ct);
    }

    public void Update(T entity)
    {
        Guard.AgainstNull(entity, nameof(entity));
        _set.Update(entity);
    }

    public void Delete(T entity)
    {
        Guard.AgainstNull(entity, nameof(entity));
        _set.Remove(entity);
    }

    public async Task<int> CountAsync(ISpecification<T>? spec = null, CancellationToken ct = default)
    {
        var query = spec is null ? _set : SpecificationEvaluator<T>.GetQuery(_set.AsQueryable(), spec);
        return await query.CountAsync(ct);
    }
}