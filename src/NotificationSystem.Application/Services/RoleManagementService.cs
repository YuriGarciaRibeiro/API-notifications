using FluentResults;
using NotificationSystem.Application.DTOs.Common;
using NotificationSystem.Application.DTOs.Roles;
using NotificationSystem.Application.Interfaces;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.Services;

public class RoleManagementService(IRoleRepository roleRepository, IPermissionRepository permissionRepository) : IRoleManagementService
{
    private readonly IRoleRepository _roleRepository = roleRepository;
    private readonly IPermissionRepository _permissionRepository = permissionRepository;

    public async Task<Result<RoleDetailDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdWithPermissionsAsync(id, cancellationToken);

        if (role == null)
            return Result.Fail<RoleDetailDto>("Role not found");

        var roleDto = MapToDetailDto(role);
        return Result.Ok(roleDto);
    }

    public async Task<Result<IEnumerable<RoleDetailDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.GetAllWithPermissionsAsync(cancellationToken);
        var roleDtos = roles.Select(MapToDetailDto);
        return Result.Ok(roleDtos);
    }

    public async Task<Result<RoleDetailDto>> CreateAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (await _roleRepository.NameExistsAsync(request.Name, cancellationToken))
            return Result.Fail<RoleDetailDto>("Role name already exists");

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IsSystemRole = false,
            CreatedAt = DateTime.UtcNow
        };

        if (request.PermissionIds.Any())
        {
            role.RolePermissions = request.PermissionIds.Select(permissionId => new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permissionId,
                GrantedAt = DateTime.UtcNow
            }).ToList();
        }

        await _roleRepository.AddAsync(role, cancellationToken);

        role = await _roleRepository.GetByIdWithPermissionsAsync(role.Id, cancellationToken);
        var roleDto = MapToDetailDto(role!);

        return Result.Ok(roleDto);
    }

    public async Task<Result<RoleDetailDto>> UpdateAsync(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdWithPermissionsAsync(id, cancellationToken);

        if (role == null)
            return Result.Fail<RoleDetailDto>("Role not found");

        if (role.IsSystemRole)
            return Result.Fail<RoleDetailDto>("Cannot modify system role");

        if (request.Name != null && request.Name != role.Name)
        {
            if (await _roleRepository.NameExistsAsync(request.Name, cancellationToken))
                return Result.Fail<RoleDetailDto>("Role name already exists");

            role.Name = request.Name;
        }

        if (request.Description != null)
            role.Description = request.Description;

        if (request.PermissionIds != null)
        {
            role.RolePermissions.Clear();
            role.RolePermissions = request.PermissionIds.Select(permissionId => new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permissionId,
                GrantedAt = DateTime.UtcNow
            }).ToList();
        }

        role.UpdatedAt = DateTime.UtcNow;
        await _roleRepository.UpdateAsync(role, cancellationToken);

        role = await _roleRepository.GetByIdWithPermissionsAsync(role.Id, cancellationToken);
        var roleDto = MapToDetailDto(role!);

        return Result.Ok(roleDto);
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(id, cancellationToken);

        if (role == null)
            return Result.Fail("Role not found");

        if (role.IsSystemRole)
            return Result.Fail("Cannot delete system role");

        if (await _roleRepository.HasUsersAsync(id, cancellationToken))
            return Result.Fail("Cannot delete role that is assigned to users");

        await _roleRepository.DeleteAsync(id, cancellationToken);
        return Result.Ok();
    }

    public async Task<Result<IEnumerable<PermissionDto>>> GetAllPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await _permissionRepository.GetAllAsync(cancellationToken);
        var permissionDtos = permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Code = p.Code,
            Name = p.Name,
            Description = p.Description,
            Category = p.Category
        });

        return Result.Ok(permissionDtos);
    }

    private static RoleDetailDto MapToDetailDto(Role role)
    {
        return new RoleDetailDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt,
            Permissions = role.RolePermissions.Select(rp => new PermissionDto
            {
                Id = rp.Permission.Id,
                Code = rp.Permission.Code,
                Name = rp.Permission.Name,
                Description = rp.Permission.Description,
                Category = rp.Permission.Category
            }).ToList()
        };
    }
}
