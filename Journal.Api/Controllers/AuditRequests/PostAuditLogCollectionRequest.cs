namespace Journal.Api.Controllers.AuditRequests;

public record PostAuditLogRequest
{
    public string ServerName { get; init; } = string.Empty;
    public string DbName { get; init; } = string.Empty;
    public string TableName { get; init; } = string.Empty;
    public string RowId { get; init; } = string.Empty;
    public string ColumnName { get; init; } = string.Empty;
    public string OldValue { get; init; } = string.Empty;
    public string NewValue { get; init; } = string.Empty;
    public string ExecutedBy { get; init; } = string.Empty;
    public DateTime ExecutedOn { get; init; } = DateTime.UtcNow;
}

public record PostAuditLogCollectionRequest(List<PostAuditLogRequest> AuditLogs);