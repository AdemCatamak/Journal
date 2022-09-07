using System.Data;
using System.Text;
using Dapper;
using Journal.Domain;
using Journal.Domain.DataStorage;
using Journal.Domain.DataStorage.Repositories;
using Journal.Domain.Exceptions;

namespace Journal.Infra.DataStorage.Repositories;

public class PostgresAuditLogRepository : IAuditLogRepository
{
    private readonly IConnectionFactory _connectionFactory;

    public PostgresAuditLogRepository(IConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InsertAsync(IEnumerable<AuditLog> auditLogs, CancellationToken cancellationToken)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();
        using IDbTransaction dbTransaction = connection.BeginTransaction();

        const string commandText = @"INSERT INTO audit_logs (server_name, db_name, table_name, row_id, column_name, old_value, new_value, executed_by, executed_on, created_on)
VALUES (@server_name, @db_name, @table_name, @row_id, @column_name, @old_value, @new_value, @executed_by, @executed_on, @created_on)";
        var parameters = auditLogs.Select(x => new
                                               {
                                                   server_name = x.ServerName,
                                                   db_name = x.DbName,
                                                   table_name = x.TableName,
                                                   row_id = x.RowId,
                                                   column_name = x.ColumnName,
                                                   old_value = x.OldValue,
                                                   new_value = x.NewValue,
                                                   executed_by = x.ExecutedBy,
                                                   executed_on = x.ExecutedOn,
                                                   created_on = x.CreatedOn
                                               })
                                  .ToArray();
        var commandDefinition = new CommandDefinition(commandText: commandText, parameters: parameters, transaction: dbTransaction, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(commandDefinition);

        dbTransaction.Commit();
    }

    public async Task<List<AuditLog>> GetAsync(string serverName, string dbName, string? tableName, string? rowId, string? columnName, long afterId, int take, CancellationToken cancellationToken)
    {
        List<AuditLog> auditLogList = await TryGetAsync(serverName, dbName, tableName, rowId, columnName, afterId, take, cancellationToken);

        if (!auditLogList.Any())
        {
            throw new NotFoundException(nameof(AuditLog));
        }

        return auditLogList;
    }

    public async Task<List<AuditLog>> TryGetAsync(string serverName, string dbName, string? tableName, string? rowId, string? columnName, long afterId, int take, CancellationToken cancellationToken)
    {
        using IDbConnection connection = await _connectionFactory.CreateConnectionAsync();

        var parameters = new DynamicParameters();
        var commandTextBuilder = new StringBuilder();
        commandTextBuilder.Append(@"SELECT id as Id, server_name as ServerName, db_name as DbName, table_name as TableName, row_id as RowId, column_name as ColumnName, old_value as OldValue, new_value as NewValue, executed_by as ExecutedBy, executed_on as ExecutedOn, created_on as CreatedOn
FROM audit_logs WHERE id > @after AND server_name = @server_name AND db_name = @db_name ");
        parameters.Add("after", afterId, DbType.Int64);
        parameters.Add("server_name", serverName, DbType.String);
        parameters.Add("db_name", dbName, DbType.String);

        if (tableName != null)
        {
            commandTextBuilder.Append(" AND table_name = @table_name ");
            parameters.Add("table_name", tableName, DbType.String);
        }

        if (rowId != null)
        {
            commandTextBuilder.Append(" AND row_id = @row_id ");
            parameters.Add("row_id", rowId, DbType.String);
        }

        if (columnName != null)
        {
            commandTextBuilder.Append(" AND column_name = @column_name ");
            parameters.Add("column_name", columnName, DbType.String);
        }

        commandTextBuilder.Append($" ORDER BY id ASC LIMIT {take} ");

        var commandText = commandTextBuilder.ToString();
        var commandDefinition = new CommandDefinition(commandText: commandText, parameters: parameters, cancellationToken: cancellationToken);
        List<AuditLog> auditLogList = (await connection.QueryAsync<AuditLog>(commandDefinition)).ToList();

        return auditLogList;
    }
}