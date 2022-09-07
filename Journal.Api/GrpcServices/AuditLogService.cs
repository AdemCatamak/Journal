using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Journal.Domain;
using Journal.Domain.DataStorage.Repositories;

namespace Journal.Api.GrpcServices;

public class AuditLogService : AuditGrpcService.AuditGrpcServiceBase
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogService(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public override async Task<Empty> CreateAuditLogs(CreateAuditLogCollectionRequest request, ServerCallContext context)
    {
        List<AuditLog> auditLogList = request.AuditLogs
                                             .Select(x => AuditLog.Create(x.ServerName, x.DbName, x.TableName, x.RowId, x.ColumnName, x.OldValue, x.NewValue, x.ExecutedBy, x.ExecutedOn.ToDateTime()))
                                             .ToList();
        await _auditLogRepository.InsertAsync(auditLogList, context.CancellationToken);

        return new Empty();
    }

    public override async Task QueryAuditLogs(QueryAuditLogCollectionRequest request, IServerStreamWriter<QueryAuditLogResponse> responseStream, ServerCallContext context)
    {
        const int fetchSize = 100;
        int remain = request.Take ?? int.MaxValue;
        do
        {
            int take = Math.Min(fetchSize, remain);
            remain -= take;
            List<AuditLog> auditLogList = await _auditLogRepository.TryGetAsync(request.ServerName, request.DbName, request.TableName, request.RowId, request.ColumnName, request.AfterId, take, context.CancellationToken);
            if (!auditLogList.Any()) remain = 0;

            List<QueryAuditLogResponse> queryAuditLogResponseList = auditLogList.Select(a => new QueryAuditLogResponse
                                                                                             {
                                                                                                 Id = a.Id,
                                                                                                 ServerName = a.ServerName,
                                                                                                 DbName = a.DbName,
                                                                                                 TableName = a.TableName,
                                                                                                 RowId = a.RowId,
                                                                                                 ColumnName = a.ColumnName,
                                                                                                 OldValue = a.OldValue,
                                                                                                 NewValue = a.NewValue,
                                                                                                 ExecutedBy = a.ExecutedBy,
                                                                                                 ExecutedOn = a.ExecutedOn.ToTimestamp(),
                                                                                                 CreatedOn = a.CreatedOn.ToTimestamp()
                                                                                             })
                                                                                .ToList();
            foreach (QueryAuditLogResponse queryAuditLogResponse in queryAuditLogResponseList)
            {
                await responseStream.WriteAsync(queryAuditLogResponse, context.CancellationToken);
            }
        } while (remain > 0);
    }
}