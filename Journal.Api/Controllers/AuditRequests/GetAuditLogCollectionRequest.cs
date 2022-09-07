namespace Journal.Api.Controllers.AuditRequests;

public record GetAuditLogCollectionRequest
{
    public string? TableName { get; init; }
    public string? RowId { get; init; }
    public string? ColumnName { get; init; }

    public long AfterId { get; set; } = 0;
    public int Take { get; set; } = 1;
}