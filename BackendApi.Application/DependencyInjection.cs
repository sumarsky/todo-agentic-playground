using Microsoft.Extensions.DependencyInjection;

namespace BackendApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<TodoApplicationService>();
        services.AddTransient<UseCases.CreateTodoUseCase>();
        services.AddTransient<UseCases.ListTodosUseCase>();
        services.AddTransient<UseCases.UpdateTitleUseCase>();
        services.AddTransient<UseCases.ToggleCompletedUseCase>();
        services.AddTransient<UseCases.DeleteTodoUseCase>();
        services.AddTransient<UseCases.BulkDeleteTodoUseCase>();

        return services;
    }
}
