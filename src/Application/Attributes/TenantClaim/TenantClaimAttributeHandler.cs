using Application.Attributes.Common;
using Application.Models;
using Application.Services;

namespace Application.Attributes.TenantClaim;

public class TenantClaimAttributeHandler(
    ILogger logger,
    AuthContext authContext) : IAttributeHandler<TenantClaimAttribute>
{
    public Task Handle<TRequest>(
        TRequest request,
        TenantClaimAttribute attribute,
        CancellationToken cancellationToken) where TRequest : notnull
    {
        if (authContext.IsSuperAdmin)
        {
            return Task.CompletedTask;
        }

        var tenantId = request.GetType()
            .GetProperty("TenantId")
            ?.GetValue(request);

        if (tenantId is null)
        {
            logger.LogWarning($"User {authContext.UserId} tried to access {typeof(TRequest).Name} without tenant id");

            throw new ResultErrorException(AuthErrors.Unauthorized);
        }

        if (!authContext.TenantIds.Contains((int)tenantId))
        {
            logger.LogWarning(
                $"User {authContext.UserId} tried to access {typeof(TRequest).Name} with invalid tenant id {tenantId}");

            throw new ResultErrorException(AuthErrors.Forbidden);
        }

        return Task.CompletedTask;
    }
}