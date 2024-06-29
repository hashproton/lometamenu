using Application.Repositories.Common;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories.Common;

public static class DbSetExtensions
{
    public static async Task<PaginatedResult<T>> ToPaginatedResultAsync<T>(
        this IQueryable<T> query,
        PaginatedQuery paginatedQuery,
        CancellationToken cancellationToken)
    {
        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = (int)Math.Ceiling(totalItems / (double)paginatedQuery.PageSize);

        var items = await query
            .Skip(paginatedQuery.PageSize * (paginatedQuery.PageNumber - 1))
            .Take(paginatedQuery.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<T>(items,
            totalItems,
            totalPages,
            paginatedQuery.PageNumber,
            paginatedQuery.PageSize);
    }

    public static IOrderedQueryable<T> ToSort<T>(
        this IQueryable<T> query,
        SortableFilter sortableFilter) where T : BaseEntity
    {
        var defaultOrder = query.OrderBy(e => e.Id);
        if (sortableFilter.OrderByField == null)
        {
            return defaultOrder;
        }

        var property = typeof(T).GetProperty(sortableFilter.OrderByField);
        if (property == null)
        {
            return defaultOrder;
        }

        if (sortableFilter.Direction == SortableFilter.SortDirection.Asc)
        {
            return query.OrderBy(e => property.GetValue(e));
        }

        return query.OrderByDescending(e => property.GetValue(e));
    }
}