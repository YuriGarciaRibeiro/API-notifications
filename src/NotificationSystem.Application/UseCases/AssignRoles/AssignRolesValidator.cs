using FluentValidation;

namespace NotificationSystem.Application.UseCases.AssignRoles;

public class AssignRolesValidator : AbstractValidator<AssignRolesCommand>
{
    public AssignRolesValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId é obrigatório");

        RuleFor(x => x.RoleIds)
            .NotEmpty().WithMessage("RoleIds é obrigatório");
    }
}
