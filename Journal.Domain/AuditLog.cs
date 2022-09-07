using Journal.Domain.Exceptions;

namespace Journal.Domain;

public class AuditLog
{
    public long Id { get; private set; }
    public string ServerName { get; private set; } = string.Empty;
    public string DbName { get; private set; } = string.Empty;
    public string TableName { get; private set; } = string.Empty;
    public string RowId { get; private set; } = string.Empty;
    public string ColumnName { get; private set; } = string.Empty;
    public string OldValue { get; private set; } = string.Empty;
    public string NewValue { get; private set; } = string.Empty;
    public string ExecutedBy { get; private set; } = string.Empty;
    public DateTime ExecutedOn { get; private set; } = DateTime.UtcNow;
    public DateTime CreatedOn { get; private set; } = DateTime.UtcNow;

    private AuditLog()
    {
    }

    public static AuditLog Create(string serverName, string dbName, string tableName, string rowId, string columnName, string oldValue, string newValue, string executedBy, DateTime executedOn)
    {
        const string emptyStringErrorMessage = "Property should not be empty";
        const string defaultPropertyErrorMessage = "Property should not be default";

        if (string.IsNullOrEmpty(serverName)) throw new PropertyValidationException(nameof(serverName), emptyStringErrorMessage);
        if (string.IsNullOrEmpty(dbName)) throw new PropertyValidationException(nameof(dbName), emptyStringErrorMessage);
        if (string.IsNullOrEmpty(tableName)) throw new PropertyValidationException(nameof(tableName), emptyStringErrorMessage);
        if (string.IsNullOrEmpty(rowId)) throw new PropertyValidationException(nameof(rowId), emptyStringErrorMessage);
        if (string.IsNullOrEmpty(columnName)) throw new PropertyValidationException(nameof(columnName), emptyStringErrorMessage);
        if (string.IsNullOrEmpty(newValue)) throw new PropertyValidationException(nameof(newValue), emptyStringErrorMessage);
        if (string.IsNullOrEmpty(executedBy)) throw new PropertyValidationException(nameof(executedBy), emptyStringErrorMessage);
        if (executedOn == default) throw new PropertyValidationException(nameof(executedOn), defaultPropertyErrorMessage);

        var auditLog = new AuditLog
                       {
                           ServerName = serverName,
                           DbName = dbName,
                           TableName = tableName,
                           RowId = rowId,
                           ColumnName = columnName,
                           OldValue = oldValue,
                           NewValue = newValue,
                           ExecutedBy = executedBy,
                           ExecutedOn = executedOn,
                           CreatedOn = DateTime.UtcNow
                       };
        return auditLog;
    }
}