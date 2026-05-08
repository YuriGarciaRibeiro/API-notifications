using NotificationSystem.Application.Contracts.Roles;

namespace NotificationSystem.Application.UseCases.GetAllRoles;

public record GetAllRolesResponse(IEnumerable<RoleDetailDto> Roles);
