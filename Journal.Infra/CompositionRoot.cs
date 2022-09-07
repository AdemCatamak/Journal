using Journal.Domain.DataStorage;
using Journal.Domain.DataStorage.Repositories;
using Journal.Infra.DataStorage;
using Journal.Infra.DataStorage.Migrations;
using Journal.Infra.DataStorage.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Journal.Infra;

public static class CompositionRoot
{
    public static IServiceCollection Register(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IDbMigrationEngine>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            string connectionString = configuration.GetConnectionString();
            var migrationEngine = new DbMigrationEngine(connectionString);
            return migrationEngine;
        });
        serviceCollection.AddSingleton<IConnectionFactory>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            string connectionString = configuration.GetConnectionString();
            var connectionFactory = new PostgresConnectionFactory(connectionString);
            return connectionFactory;
        });
        serviceCollection.AddScoped<IAuditLogRepository, PostgresAuditLogRepository>();
        
        return serviceCollection;
    }

    private static string GetConnectionString(this IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Postgres");
        return connectionString;
    }
}