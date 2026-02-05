using FluentValidation;
using NotificationSystem.Application.Validators;

namespace NotificationSystem.Application.UseCases.Register;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido");

        RuleFor(x => x.Password)
            .StrongPassword();

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Nome completo é obrigatório")
            .MinimumLength(3).WithMessage("Nome completo deve ter no mínimo 3 caracteres");

        RuleFor(x => x.RoleIds)
            .NotEmpty().WithMessage("Ao menos um role deve ser atribuído");
    }
}
