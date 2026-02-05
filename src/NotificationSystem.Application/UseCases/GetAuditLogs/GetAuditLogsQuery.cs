using FluentResults;
using MediatR;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.GetAuditLogs;

/// <summary>
/// Query to retrieve audit logs with advanced filtering and pagination.
/// All filter parameters are optional.
/// </summary>
/// <param name="EntityName">Optional: filter by entity type (e.g., "User", "Notification")</param>
/// <param name="EntityId">Optional: filter by specific entity ID</param>
/// <param name="UserId">Optional: filter by user who performed the action</param>
/// <param name="ActionType">Optional: filter by action type (Created, Updated, Deleted)</param>
/// <param name="StartDate">Optional: filter logs from this date onwards (UTC)</param>
/// <param name="EndDate">Optional: filter logs up to this date (UTC)</param>
/// <param name="PageNumber">Page number (1-based, default: 1)</param>
/// <param name="PageSize">Number of records per page (default: 20, max: 100)</param>
public record GetAuditLogsQuery(
    string? EntityName = null,
    string? EntityId = null,
    Guid? UserId = null,
    AuditAction? ActionType = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<GetAuditLogsResponse>>;
