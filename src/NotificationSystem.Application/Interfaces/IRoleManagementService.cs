using FluentResults;
using NotificationSystem.Application.DTOs.Roles;

namespace NotificationSystem.Application.Interfaces;

public interface IRoleManagementService
{
    Task<Result<RoleDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<RoleDetailDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<RoleDetailDto>> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task<Result<RoleDetailDto>> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<PermissionDto>>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
}
