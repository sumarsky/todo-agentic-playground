using BackendApi.Application.Ports;
using BackendApi.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BackendApi.Logging;

public static class DependencyInjection
{
    public static IServiceCollection AddPostgresLogger(this IServiceCollection services)
    {
        services.AddSingleton<ILoggerProvider, PostgresLoggerProvider>(sp =>
        {
            var logStore = sp.GetRequiredService<ILogStore>();
            return new PostgresLoggerProvider(logStore);
        });

        return services;
    }
}
