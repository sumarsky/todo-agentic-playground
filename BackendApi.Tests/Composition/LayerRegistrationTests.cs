using BackendApi.Application;
using BackendApi.Application.UseCases;
using BackendApi.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BackendApi.Tests.Composition;

public class LayerRegistrationTests
{
    [Fact]
    public void AddApplicationAndAddInfrastructure_ResolveUseCases()
    {
        var services = new ServiceCollection();

        services.AddApplication();
        services.AddInfrastructure();

        using var provider = services.BuildServiceProvider();

        var createUseCase = provider.GetRequiredService<CreateTodoUseCase>();
        var listUseCase = provider.GetRequiredService<ListTodosUseCase>();

        Assert.NotNull(createUseCase);
        Assert.NotNull(listUseCase);
    }
}
