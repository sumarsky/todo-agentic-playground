using BackendApi.Application;
using BackendApi.Application.Ports;
using BackendApi.Application.UseCases;
using BackendApi.Infrastructure;
using BackendApi.Storage.Postgres;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BackendApi.Tests.Composition;

public class LayerRegistrationTests
{
    [Fact]
    public void AddApplicationAndAddInfrastructure_ResolveUseCases()
    {
        var services = new ServiceCollection();

        services.AddApplication();
        services.AddSingleton<ITodoRepository, InMemoryTodoRepository>();

        using var provider = services.BuildServiceProvider();

        var createUseCase = provider.GetRequiredService<CreateTodoUseCase>();
        var listUseCase = provider.GetRequiredService<ListTodosUseCase>();

        Assert.NotNull(createUseCase);
        Assert.NotNull(listUseCase);
    }

    [Fact]
    public void AddPostgresStorage_RegistersRepository()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new KeyValuePair<string, string?>[]
            {
                new("ConnectionStrings:Default", "Host=localhost;Database=test;Username=test;Password=test")
            }!)
            .Build();

        services.AddApplication();
        services.AddPostgresStorage(configuration);

        using var provider = services.BuildServiceProvider();

        var repository = provider.GetService<ITodoRepository>();
        Assert.NotNull(repository);
    }
}
