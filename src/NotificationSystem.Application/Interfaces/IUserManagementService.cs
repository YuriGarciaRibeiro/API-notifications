using FluentResults;
using NotificationSystem.Application.DTOs.Users;

namespace NotificationSystem.Application.Interfaces;

public interface IUserManagementService
{
    Task<Result<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<UserDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<UserDto>> CreateAsync(string email, string password, string fullName, List<Guid> roleIds, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<Result> AssignRolesAsync(Guid userId, List<Guid> roleIds, CancellationToken cancellationToken = default);
}
