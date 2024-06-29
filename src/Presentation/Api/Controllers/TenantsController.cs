using Api.Extensions;
using Application.Repositories;
using Application.UseCases.Tenants.Commands;
using Application.UseCases.Tenants.Queries;
using Application.UseCases.Tenants.Queries.Common;

namespace Api.Controllers;

public class TenantsController(
    ISender mediator) : BaseController
{
    [HttpPost]
    public async Task<ActionResult> CreateTenant([FromBody] CreateTenantCommand command)
    {
        var result = await mediator.Send(command);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetTenantById), new { id = result.Value }, result.Value)
            : result.ToActionResult();
    }

    [HttpGet]
    public async Task<ActionResult<PaginatedResult<TenantQueryResponse>>> GetAllTenants(
        [FromQuery] TenantFilter filter,
        [FromQuery] PaginatedQuery paginatedQuery,
        [FromQuery] SortableFilter sortableFilter)
    {
        var result = await mediator.Send(new GetAllTenantsQuery(filter, paginatedQuery, sortableFilter));

        return result.IsSuccess ? Ok(result.Value) : result.ToActionResult();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TenantQueryResponse>> GetTenantById([FromRoute] int id)
    {
        var result = await mediator.Send(new GetTenantByIdQuery(id));

        return result.IsSuccess ? Ok(result.Value) : result.ToActionResult();
    }

    [HttpGet("info")]
    public async Task<ActionResult<GetTenantsInfoQueryResponse>> GetTenantsInfo()
    {
        var result = await mediator.Send(new GetTenantsInfoQuery());

        return result.IsSuccess ? Ok(result.Value) : result.ToActionResult();
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> UpdateTenant(
        [FromRoute] int id,
        [FromBody] UpdateTenantCommand command)
    {
        command.Id = id;

        var result = await mediator.Send(command);

        return result.IsSuccess ? NoContent() : result.ToActionResult();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteTenant([FromRoute] int id)
    {
        var result = await mediator.Send(new DeleteTenantCommand(id));

        return result.IsSuccess ? NoContent() : result.ToActionResult();
    }
}