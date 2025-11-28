using FreshMarket.Domain.Interfaces.Repositories;
using FreshMarket.Infrastructure.Data;
using FreshMarket.Infrastructure.Helpers;
using FreshMarket.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FreshMarket.Infrastructure.Repositories;

public class Repository<T>(FreshMarketDbContext context, ILogger<Repository<T>> logger)
    : IRepository<T> where T : class
{
    private readonly DbSet<T> _set = context.Set<T>();

    public async Task<T?> GetByIdAsync(object id, CancellationToken ct = default)
    {
        Guard.AgainstNull(id, nameof(id));

        return await ExecutionHelper.ExecuteAsync(
            () => _set.FindAsync([id], ct).AsTask(),
            logger,
            $"Get {typeof(T).Name} by Id",
            new { Id = id }
        );
    }

    public async Task<IReadOnlyList<T>> ListAllAsync(CancellationToken ct = default)
        => await ExecutionHelper.ExecuteAsync(
            () => _set.ToListAsync(ct),
            logger,
            $"List all {typeof(T).Name}"
        );

    public async Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        Guard.AgainstNull(spec, nameof(spec));
        var query = SpecificationEvaluator<T>.GetQuery(_set.AsQueryable(), spec);
        return await ExecutionHelper.ExecuteAsync(
            () => query.ToListAsync(ct),
            logger,
            $"List {typeof(T).Name} with specification"
        );
    }

    public async Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct = default)
    {
        Guard.AgainstNull(spec, nameof(spec));
        var query = SpecificationEvaluator<T>.GetQuery(_set.AsQueryable(), spec);

        return await ExecutionHelper.ExecuteAsync(
            () => query.FirstOrDefaultAsync(ct),
            logger,
            $"First {typeof(T).Name} with specification"
        );
    }

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        Guard.AgainstNull(entity, nameof(entity));
        return await ExecutionHelper.ExecuteAsync(
            async () =>
            {
                await _set.AddAsync(entity, ct);
                return entity;
            },
            logger,
            $"Add {typeof(T).Name}"
        );
    }

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
    {
        Guard.AgainstNullOrEmpty(entities, nameof(entities));
        await ExecutionHelper.ExecuteAsync(
            () => _set.AddRangeAsync(entities, ct),
            logger,
            $"Add range {typeof(T).Name}",
            new { Count = entities.Count() }
        );
    }

    public void Update(T entity)
    {
        Guard.AgainstNull(entity, nameof(entity));
        ExecutionHelper.Execute(
            () => _set.Update(entity),
            logger,
            $"Update {typeof(T).Name}"
        );
    }

    public void Delete(T entity)
    {
        Guard.AgainstNull(entity, nameof(entity));
        ExecutionHelper.Execute(
            () => _set.Remove(entity),
            logger,
            $"Delete {typeof(T).Name}"
        );
    }

    public async Task<int> CountAsync(ISpecification<T>? spec = null, CancellationToken ct = default)
    {
        var query = spec is null ? _set : SpecificationEvaluator<T>.GetQuery(_set.AsQueryable(), spec);
        return await ExecutionHelper.ExecuteAsync(
            () => query.CountAsync(ct),
            logger,
            $"Count {typeof(T).Name}"
        );
    }
}
