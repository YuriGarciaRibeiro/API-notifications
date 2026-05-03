using FluentValidation;

namespace NotificationSystem.Application.UseCases.UpdateProviderConfiguration;

public class UpdateProviderConfigurationValidator : AbstractValidator<UpdateProviderConfigurationCommand>
{
    public UpdateProviderConfigurationValidator()
    {
        RuleFor(x => x.ProviderId)
            .NotEmpty();

        RuleFor(x => x.Configuration.ValueKind)
            .Equal(System.Text.Json.JsonValueKind.Object)
            .WithMessage("Configuration must be a JSON object");
    }
}
