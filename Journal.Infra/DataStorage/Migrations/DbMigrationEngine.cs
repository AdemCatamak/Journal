using System.Reflection;
using FluentMigrator.Runner;
using Journal.Domain.DataStorage;
using Microsoft.Extensions.DependencyInjection;

namespace Journal.Infra.DataStorage.Migrations;

public class DbMigrationEngine : IDbMigrationEngine
{
    private readonly string _connectionString;

    public DbMigrationEngine(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void Migrate()
    {
        IServiceProvider serviceProvider = CreateServices(_connectionString, this.GetType().Assembly);

        using IServiceScope scope = serviceProvider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
    }

    private static IServiceProvider CreateServices(string dbConnectionString, Assembly assemblies)
    {
        IServiceCollection? serviceCollection = new ServiceCollection()
                                               .AddFluentMigratorCore()
                                               .AddLogging(lb => lb.AddFluentMigratorConsole())
                                               .ConfigureRunner(rb =>
                                                {
                                                    rb
                                                       .AddPostgres()
                                                       .WithGlobalConnectionString(dbConnectionString)
                                                       .ScanIn(assemblies).For.Migrations();
                                                });

        return serviceCollection.BuildServiceProvider(false);
    }
}