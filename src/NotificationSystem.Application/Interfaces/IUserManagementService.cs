using FluentResults;
using NotificationSystem.Application.DTOs.Users;

namespace NotificationSystem.Application.Interfaces;

public interface IUserManagementService
{
    public Task<Result<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<Result<IEnumerable<UserDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    public Task<Result<UserDto>> CreateAsync(string email, string password, string fullName, List<Guid> roleIds, CancellationToken cancellationToken = default);
    public Task<Result<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    public Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    public Task<Result> AssignRolesAsync(Guid userId, List<Guid> roleIds, CancellationToken cancellationToken = default);
}
