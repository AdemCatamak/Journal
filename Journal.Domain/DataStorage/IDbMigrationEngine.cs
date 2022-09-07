namespace Journal.Domain.DataStorage;

public interface IDbMigrationEngine
{
    void Migrate();
}
