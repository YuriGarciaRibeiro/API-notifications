using FluentResults;
using NotificationSystem.Application.Common.Errors;
using NotificationSystem.Application.DTOs.Common;
using NotificationSystem.Application.DTOs.Roles;
using NotificationSystem.Application.DTOs.Users;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.Services;

public class UserManagementService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUserService,
    INotificationRepository notificationRepository) : IUserManagementService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IPasswordHasher _passwordHasher = passwordHasher;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly INotificationRepository _notificationRepository = notificationRepository;

    public async Task<Result<UserDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(id, cancellationToken);

        if (user == null)
            return Result.Fail<UserDto>(new NotFoundError("User", id));

        var userDto = MapToDto(user);
        return Result.Ok(userDto);
    }

    public async Task<Result<IEnumerable<UserDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllWithRolesAsync(cancellationToken);
        var userDtos = users.Select(MapToDto);
        return Result.Ok(userDtos);
    }

    public async Task<Result<UserDto>> CreateAsync(string email, string password, string fullName, List<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        if (await _userRepository.EmailExistsAsync(email, cancellationToken))
            return Result.Fail<UserDto>(new ConflictError("User", $"email '{email}' already in use"));

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(password),
            FullName = fullName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        if (roleIds.Any())
        {
            user.UserRoles = roleIds.Select(roleId => new UserRole
            {
                UserId = user.Id,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow
            }).ToList();
        }

        await _userRepository.AddAsync(user, cancellationToken);

        var retrievedUser = await _userRepository.GetByIdWithRolesAsync(user.Id, cancellationToken);
        var userDto = MapToDto(retrievedUser!);

        var welcomeNotification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = _currentUserService.UserId ?? Guid.Empty,
            CreatedAt = DateTime.UtcNow,
            Channels = new List<NotificationChannel>
            {
                new EmailChannel
                {
                    Id = Guid.NewGuid(),
                    NotificationId = Guid.NewGuid(),
                    To = retrievedUser!.Email,
                    Subject = "Welcome to NotificationSystem!",
                    Body = $"Hello {retrievedUser.FullName}, welcome to WUPHF",
                    IsBodyHtml = false
                }
            }
        };

       await _notificationRepository.AddAsync(welcomeNotification);

        welcomeNotification.PublishToAllChannels();

        await _notificationRepository.UpdateAsync(welcomeNotification);
        return Result.Ok(userDto);
    }

    public async Task<Result<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(id, cancellationToken);

        if (user == null)
            return Result.Fail<UserDto>(new NotFoundError("User", id));

        if (request.Email != null && request.Email != user.Email)
        {
            if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
                return Result.Fail<UserDto>(new ConflictError("User", $"email '{request.Email}' already in use"));

            user.Email = request.Email;
        }

        if (request.FullName != null)
            user.FullName = request.FullName;

        if (request.IsActive.HasValue)
            user.IsActive = request.IsActive.Value;

        if (request.RoleIds != null)
        {
            user.UserRoles.Clear();
            user.UserRoles = request.RoleIds.Select(roleId => new UserRole
            {
                UserId = user.Id,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow
            }).ToList();
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);

        user = await _userRepository.GetByIdWithRolesAsync(user.Id, cancellationToken);
        var userDto = MapToDto(user!);

        return Result.Ok(userDto);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;

        if (currentUserId.HasValue && currentUserId.Value == id)
            return Result.Fail(new ForbiddenError("Cannot delete your own user account"));

        var user = await _userRepository.GetByIdAsync(id, cancellationToken);

        if (user == null)
            return Result.Fail(new NotFoundError("User", id));

        await _userRepository.DeleteAsync(id, cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
            return Result.Fail(new NotFoundError("User", userId));

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            return Result.Fail(new ValidationError("CurrentPassword", "Current password is incorrect"));

        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> AssignRolesAsync(Guid userId, List<Guid> roleIds, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(userId, cancellationToken);

        if (user == null)
            return Result.Fail(new NotFoundError("User", userId));

        user.UserRoles.Clear();
        user.UserRoles = roleIds.Select(roleId => new UserRole
        {
            UserId = user.Id,
            RoleId = roleId,
            AssignedAt = DateTime.UtcNow
        }).ToList();

        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken);

        return Result.Ok();
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FullName = user.FullName,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            Roles = user.UserRoles.Select(ur => new RoleDto
            {
                Id = ur.Role.Id,
                Name = ur.Role.Name,
                Description = ur.Role.Description,
                IsSystemRole = ur.Role.IsSystemRole,
                CreatedAt = ur.Role.CreatedAt,
                Permissions = ur.Role.RolePermissions.Select(rp => new PermissionDto
                {
                    Id = rp.Permission.Id,
                    Code = rp.Permission.Code,
                    Name = rp.Permission.Name,
                    Description = rp.Permission.Description,
                    Category = rp.Permission.Category
                }).ToList()
            }).ToList()
        };
    }
}
