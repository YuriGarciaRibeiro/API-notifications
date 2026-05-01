using FluentValidation;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.UseCases.CreateProviderFromUpload;

public class CreateProviderFromUploadValidator : AbstractValidator<CreateProviderFromUploadCommand>
{
    public CreateProviderFromUploadValidator()
    {
        RuleFor(x => x.ChannelType)
            .NotEmpty()
            .WithMessage("Channel type is required")
            .Must(value => Enum.TryParse<ChannelType>(value, true, out _))
            .WithMessage("Invalid channelType");

        RuleFor(x => x.Provider)
            .NotEmpty()
            .WithMessage("Provider is required")
            .Must(value => Enum.TryParse<ProviderType>(value, true, out _))
            .WithMessage("Invalid provider");

        RuleFor(x => x.FileStream)
            .NotNull()
            .WithMessage("File is required");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File is required");

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .WithMessage("File is required and cannot be empty");

        RuleFor(x => x.IsActive)
            .Must(value => string.IsNullOrWhiteSpace(value) || bool.TryParse(value, out _))
            .WithMessage("Invalid isActive. Use true or false.");

        RuleFor(x => x.IsPrimary)
            .Must(value => string.IsNullOrWhiteSpace(value) || bool.TryParse(value, out _))
            .WithMessage("Invalid isPrimary. Use true or false.");
    }
}
