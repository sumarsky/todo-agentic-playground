using BackendApi.Application;
using BackendApi.Application.Ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace BackendApi.Storage.Postgres;

public static class DependencyInjection
{
    public static IServiceCollection AddPostgresStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");

        services.AddSingleton<NpgsqlDataSource>(NpgsqlDataSource.Create(connectionString));
        services.AddTransient<ITodoRepository, PostgresTodoRepository>();
        services.AddTransient<ILogStore, PostgresLogStore>();
        services.AddHostedService<MigrationRunner>();

        return services;
    }
}
