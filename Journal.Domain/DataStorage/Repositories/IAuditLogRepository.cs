namespace Journal.Domain.DataStorage.Repositories;

public interface IAuditLogRepository
{
    Task InsertAsync(IEnumerable<AuditLog> auditLogs, CancellationToken cancellationToken);
    Task<List<AuditLog>> GetAsync(string serverName, string dbName, string? tableName, string? rowId, string? columnName, long afterId, int take, CancellationToken cancellationToken);
    Task<List<AuditLog>> TryGetAsync(string serverName, string dbName, string? tableName, string? rowId, string? columnName, long afterId, int take, CancellationToken cancellationToken);
}