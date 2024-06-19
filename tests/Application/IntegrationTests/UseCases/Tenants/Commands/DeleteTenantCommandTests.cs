using Application.UseCases.Tenants.Commands;

namespace IntegrationTests.UseCases.Tenants.Commands;

[TestClass]
public class DeleteTenantCommandTests : BaseIntegrationTest
{
    [TestMethod]
    public async Task DeleteTenant_Success()
    {
        // Arrange: Create a tenant to delete
        var tenantId = await TenantRepository.AddAsync(new()
        {
            Name = "Tenant to delete"
        }, default);

        // Act: Delete the tenant
        var deleteCommand = new DeleteTenantCommand(tenantId);
        await Mediator.Send(deleteCommand);

        // Assert: Verify the tenant was deleted
        var tenant = await TenantRepository.GetByIdAsync(tenantId, default);
        Assert.IsNull(tenant);
    }
}