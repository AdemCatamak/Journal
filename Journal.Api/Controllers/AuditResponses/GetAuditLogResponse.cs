namespace Journal.Api.Controllers.AuditResponses;

public record GetAuditLogResponse
{
    public string ServerName { get; private set; } 
    public string DbName { get; private set; }
    public string TableName { get; private set; }
    public string RowId { get; private set; }
    public string ColumnName { get; private set; }
    public string OldValue { get; private set; } 
    public string NewValue { get; private set; } 
    public string ExecutedBy { get; private set; }
    public DateTime ExecutedOn { get; private set; }
    public DateTime CreatedOn { get; private set; }

    public GetAuditLogResponse(string serverName, string dbName, string tableName, string rowId, string columnName, string oldValue, string newValue, string executedBy, DateTime executedOn, DateTime createdOn)
    {
        ServerName = serverName;
        DbName = dbName;
        TableName = tableName;
        RowId = rowId;
        ColumnName = columnName;
        OldValue = oldValue;
        NewValue = newValue;
        ExecutedBy = executedBy;
        ExecutedOn = executedOn;
        CreatedOn = createdOn;
    }
};