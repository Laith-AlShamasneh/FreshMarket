using FreshMarket.Domain.Interfaces.Repositories;
using System.Linq.Expressions;

namespace FreshMarket.Application.Services.Implementations.Specifications;

/// <summary>
/// Base specification class for building queries.
/// Located in Application Layer.
/// </summary>
public abstract class BaseSpecification<T>(Expression<Func<T, bool>>? criteria = null) : ISpecification<T> where T : class
{
    public Expression<Func<T, bool>>? Criteria { get; } = criteria;
    public List<Expression<Func<T, object>>> Includes { get; } = [];
    public List<string> IncludeStrings { get; } = [];
    public Expression<Func<T, object>>? OrderBy { get; protected set; }
    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }
    public int? Take { get; protected set; }
    public int? Skip { get; protected set; }
    public bool IsPagingEnabled { get; protected set; }

    IReadOnlyList<Expression<Func<T, object>>> ISpecification<T>.Includes => Includes;

    IReadOnlyList<string> ISpecification<T>.IncludeStrings => IncludeStrings;

    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected virtual void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    protected virtual void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    protected virtual void ApplyOrdering(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    protected virtual void ApplyOrderingDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
    }
}
