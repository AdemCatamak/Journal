using System.Data;
using Journal.Domain.DataStorage;
using Npgsql;

namespace Journal.Infra.DataStorage;

public class PostgresConnectionFactory : IConnectionFactory
{
    private readonly string _connectionStr;

    public PostgresConnectionFactory(string connectionStr)
    {
        _connectionStr = connectionStr;
    }

    public async Task<IDbConnection> CreateConnectionAsync()
    {
        var connection = new NpgsqlConnection(_connectionStr);
        await connection.OpenAsync();
        return connection;
    }
}