using FluentResults;
using MediatR;

namespace NotificationSystem.Application.UseCases.GetAuditLogById;

/// <summary>
/// Query to retrieve details of a specific audit log entry by ID.
/// </summary>
/// <param name="Id">ID of the audit log entry to retrieve</param>
public record GetAuditLogByIdQuery(Guid Id) : IRequest<Result<GetAuditLogByIdResponse>>;
