namespace Journal.Api.Controllers.AuditResponses;

public record GetAuditLogCollectionResponse(List<GetAuditLogResponse> AuditLogs);