using System.Data;

namespace Journal.Domain.DataStorage;

public interface IConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync();
}