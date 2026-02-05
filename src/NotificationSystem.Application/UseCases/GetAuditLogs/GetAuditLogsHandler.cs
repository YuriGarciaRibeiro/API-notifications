using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.AuditLogs;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetAuditLogs;

/// <summary>
/// Handler for GetAuditLogsQuery.
/// Retrieves paginated audit logs with advanced filtering.
/// </summary>
public class GetAuditLogsHandler(IAuditLogRepository auditLogRepository)
    : IRequestHandler<GetAuditLogsQuery, Result<GetAuditLogsResponse>>
{
    private readonly IAuditLogRepository _auditLogRepository = auditLogRepository
        ?? throw new ArgumentNullException(nameof(auditLogRepository));

    public async Task<Result<GetAuditLogsResponse>> Handle(
        GetAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        var (entityName, entityId, userId, actionType, startDate, endDate, pageNumber, pageSize) = request;

        // Retrieve filtered audit logs
        var auditLogs = await _auditLogRepository.GetFilteredAsync(
            entityName,
            entityId,
            userId,
            actionType,
            startDate,
            endDate,
            pageNumber,
            pageSize,
            cancellationToken);

        // Get total count for pagination
        var totalCount = await _auditLogRepository.GetFilteredCountAsync(
            entityName,
            entityId,
            userId,
            actionType,
            startDate,
            endDate,
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

        var response = new GetAuditLogsResponse(
            AuditLogs: auditLogDtos,
            TotalCount: totalCount,
            PageNumber: pageNumber,
            PageSize: pageSize
        );

        return Result.Ok(response);
    }
}
