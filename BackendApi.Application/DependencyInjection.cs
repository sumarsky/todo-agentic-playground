using BackendApi.Application.Ports;
using BackendApi.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BackendApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddTransient<UseCases.CreateTodoUseCase>();
        services.AddTransient<UseCases.ListTodosUseCase>();
        services.AddTransient<UseCases.UpdateTitleUseCase>();
        services.AddTransient<UseCases.ToggleCompletedUseCase>();
        services.AddTransient<UseCases.DeleteTodoUseCase>();
        services.AddTransient<UseCases.BulkDeleteTodoUseCase>();

        services.AddTransient<IMetricsCalculator, DefaultMetricsCalculator>();

        return services;
    }
}
