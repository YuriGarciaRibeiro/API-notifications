using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.GetEntityAuditHistory;

/// <summary>
/// Query to retrieve the complete audit history (all changes) for a specific entity.
/// Shows all create, update, and delete operations for one entity record.
/// </summary>
/// <param name="EntityName">Type of entity (e.g., "User", "Notification", "Role")</param>
/// <param name="EntityId">Specific ID of the entity</param>
/// <param name="PageNumber">Page number for pagination (1-based, default: 1)</param>
/// <param name="PageSize">Number of records per page (default: 20, max: 100)</param>
public record GetEntityAuditHistoryQuery(
    string EntityName,
    string EntityId,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<GetEntityAuditHistoryResponse>>;
