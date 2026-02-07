using FluentResults;
using NotificationSystem.Application.DTOs.Common;
using NotificationSystem.Application.DTOs.Roles;

namespace NotificationSystem.Application.Interfaces;

public interface IRoleManagementService
{
    public Task<Result<RoleDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<Result<IEnumerable<RoleDetailDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    public Task<Result<RoleDetailDto>> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
    public Task<Result<RoleDetailDto>> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default);
    public Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<Result<IEnumerable<PermissionDto>>> GetAllPermissionsAsync(CancellationToken cancellationToken = default);
}
