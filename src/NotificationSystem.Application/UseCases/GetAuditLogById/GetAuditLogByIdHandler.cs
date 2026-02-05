using FluentResults;
using MediatR;
using NotificationSystem.Application.DTOs.AuditLogs;
using NotificationSystem.Application.Interfaces;

namespace NotificationSystem.Application.UseCases.GetAuditLogById;

/// <summary>
/// Handler for GetAuditLogByIdQuery.
/// Retrieves complete details of a specific audit log entry.
/// </summary>
public class GetAuditLogByIdHandler(IAuditLogRepository auditLogRepository)
    : IRequestHandler<GetAuditLogByIdQuery, Result<GetAuditLogByIdResponse>>
{
    private readonly IAuditLogRepository _auditLogRepository = auditLogRepository
        ?? throw new ArgumentNullException(nameof(auditLogRepository));

    public async Task<Result<GetAuditLogByIdResponse>> Handle(
        GetAuditLogByIdQuery request,
        CancellationToken cancellationToken)
    {
        var auditLog = await _auditLogRepository.GetByIdAsync(request.Id, cancellationToken);

        if (auditLog is null)
        {
            return Result.Fail<GetAuditLogByIdResponse>("Audit log not found");
        }

        var auditLogDto = new AuditLogDto
        {
            Id = auditLog.Id,
            UserId = auditLog.UserId,
            UserEmail = auditLog.UserEmail,
            EntityName = auditLog.EntityName,
            EntityId = auditLog.EntityId,
            ActionType = auditLog.ActionType,
            OldValues = auditLog.OldValues,
            NewValues = auditLog.NewValues,
            ChangedProperties = auditLog.ChangedProperties,
            Timestamp = auditLog.Timestamp,
            IpAddress = auditLog.IpAddress,
            UserAgent = auditLog.UserAgent,
            RequestPath = auditLog.RequestPath
        };

        return Result.Ok(new GetAuditLogByIdResponse(auditLogDto));
    }
}
