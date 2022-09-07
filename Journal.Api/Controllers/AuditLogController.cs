using Journal.Api.Controllers.AuditRequests;
using Journal.Api.Controllers.AuditResponses;
using Journal.Domain;
using Journal.Domain.DataStorage.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Journal.Api.Controllers;

[ApiController]
public class AuditLogController : ControllerBase
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogController(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    [HttpPost("audit-logs")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Post([FromBody] PostAuditLogCollectionRequest request, CancellationToken cancellationToken)
    {
        List<AuditLog> auditLogList = request.AuditLogs
                                             .Select(x => AuditLog.Create(x.ServerName, x.DbName, x.TableName, x.RowId, x.ColumnName, x.OldValue, x.NewValue, x.ExecutedBy, x.ExecutedOn))
                                             .ToList();

        await _auditLogRepository.InsertAsync(auditLogList, cancellationToken);

        return StatusCode(StatusCodes.Status204NoContent);
    }

    [HttpGet("servers/{serverName}/dbs/{dbName}/audit-logs")]
    [ProducesResponseType(typeof(GetAuditLogCollectionResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAsync([FromRoute] string serverName, [FromRoute] string dbName, [FromQuery] GetAuditLogCollectionRequest request, CancellationToken cancellationToken)
    {
        List<AuditLog> auditLogList = await _auditLogRepository.GetAsync(serverName, dbName, request.TableName, request.RowId, request.ColumnName, request.AfterId, request.Take, cancellationToken);

        List<GetAuditLogResponse> auditLogResponseList = auditLogList.Select(a => new GetAuditLogResponse(a.ServerName, a.DbName, a.TableName, a.RowId, a.ColumnName, a.OldValue, a.NewValue, a.ExecutedBy, a.ExecutedOn, a.CreatedOn))
                                                                     .ToList();
        var response = new GetAuditLogCollectionResponse(auditLogResponseList);

        return StatusCode(StatusCodes.Status200OK, response);
    }
}