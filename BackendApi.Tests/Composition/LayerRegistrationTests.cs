using BackendApi.Application;
using BackendApi.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BackendApi.Tests.Composition;

public class LayerRegistrationTests
{
    [Fact]
    public void AddApplicationAndAddInfrastructure_ResolveTodoApplicationService()
    {
        var services = new ServiceCollection();

        services.AddApplication();
        services.AddInfrastructure();

        using var provider = services.BuildServiceProvider();

        var service = provider.GetRequiredService<TodoApplicationService>();

        Assert.NotNull(service);
    }
}
