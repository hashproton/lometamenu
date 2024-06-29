using Application.Repositories;
using Application.Repositories.Common;
using Domain.Entities;
using Infra.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Infra.Repositories;

internal sealed class TenantRepository(AppDbContext context) : GenericRepository<Tenant>(context), ITenantRepository
{
    private readonly AppDbContext _context = context;

    public async Task<PaginatedResult<Tenant>> GetAllFilteredAsync(
        TenantFilter? filter,
        PaginatedQuery paginatedQuery,
        SortableFilter sortableFilter,
        CancellationToken cancellationToken)
    {
        IQueryable<Tenant> query = _context.Tenants;

        if (filter?.Id != null)
        {
            query = query.Where(t => t.Id == filter.Id);
        }

        if (filter?.Name != null)
        {
            query = query.Where(t => t.Name.Contains(filter.Name));
        }

        if (filter?.Status != null)
        {
            query = query.Where(t => t.Status == filter.Status);
        }

        return await query
            .ToSort(sortableFilter)
            .ToPaginatedResultAsync(paginatedQuery, cancellationToken);
    }

    public Task<Tenant?> GetTenantByNameAsync(string name, CancellationToken cancellationToken)
    {
        return _context.Tenants.FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
    }

    public Task<List<TenantMetadata>> GetTenantsStatusAsync(CancellationToken cancellationToken)
    {
        return _context.Tenants
            .GroupBy(t => t.Status)
            .Select(g => new TenantMetadata
            {
                Status = g.Key,
                Count = g.Count()
            }).ToListAsync(cancellationToken);
    }
}