using FluentValidation;
using NotificationSystem.Application.DTOs.Users;

namespace NotificationSystem.Application.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required");

        RuleFor(x => x.NewPassword)
            .StrongPassword()
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from current password");
    }
}
