using FluentValidation;

namespace NotificationSystem.Application.UseCases.CreateProviderFromFile;

public class CreateProviderFromFileValidator : AbstractValidator<CreateProviderFromFileCommand>
{
    private const long MaxFileSizeBytes = 1024 * 1024; // 1MB
    private static readonly string[] AllowedExtensions = { ".json" };

    public CreateProviderFromFileValidator()
    {
        RuleFor(x => x.ChannelType)
            .IsInEnum()
            .WithMessage("Invalid channel type");

        RuleFor(x => x.Provider)
            .IsInEnum()
            .WithMessage("Invalid provider type");

        RuleFor(x => x.FileStream)
            .NotNull()
            .WithMessage("Credentials file is required");

        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required")
            .Must(fileName => AllowedExtensions.Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            .WithMessage($"Only {string.Join(", ", AllowedExtensions)} files are allowed");

        RuleFor(x => x.FileSize)
            .GreaterThan(0)
            .WithMessage("File cannot be empty")
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage($"File size must not exceed {MaxFileSizeBytes / 1024}KB");
    }
}
