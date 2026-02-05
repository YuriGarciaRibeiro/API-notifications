using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.AuditLogs;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetEntityAuditHistory;

/// <summary>
/// Handler for GetEntityAuditHistoryQuery.
/// Retrieves the complete audit trail for a specific entity record.
/// </summary>
public class GetEntityAuditHistoryHandler(IAuditLogRepository auditLogRepository)
    : IRequestHandler<GetEntityAuditHistoryQuery, Result<GetEntityAuditHistoryResponse>>
{
    private readonly IAuditLogRepository _auditLogRepository = auditLogRepository
        ?? throw new ArgumentNullException(nameof(auditLogRepository));

    public async Task<Result<GetEntityAuditHistoryResponse>> Handle(
        GetEntityAuditHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var (entityName, entityId, pageNumber, pageSize) = request;

        // Retrieve audit logs for the specific entity
        var auditLogs = await _auditLogRepository.GetByEntityAsync(
            entityName,
            entityId,
            pageNumber,
            pageSize,
            cancellationToken);

        // Get total count for pagination
        var totalCount = await _auditLogRepository.GetEntityAuditCountAsync(
            entityName,
            entityId,
            cancellationToken);

        // Map to DTOs
        var auditLogDtos = auditLogs.Select(a => new AuditLogDto
        {
            Id = a.Id,
            UserId = a.UserId,
            UserEmail = a.UserEmail,
            EntityName = a.EntityName,
            EntityId = a.EntityId,
            ActionType = a.ActionType,
            OldValues = a.OldValues,
            NewValues = a.NewValues,
            ChangedProperties = a.ChangedProperties,
            Timestamp = a.Timestamp,
            IpAddress = a.IpAddress,
            UserAgent = a.UserAgent,
            RequestPath = a.RequestPath
        }).ToList();

        var response = new GetEntityAuditHistoryResponse(
            AuditLogs: auditLogDtos,
            TotalCount: totalCount,
            PageNumber: pageNumber,
            PageSize: pageSize
        );

        return Result.Ok(response);
    }
}
