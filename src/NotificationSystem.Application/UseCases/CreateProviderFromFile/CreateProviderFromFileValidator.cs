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

        RuleFor(x => x.CredentialsFile)
            .NotNull()
            .WithMessage("Credentials file is required");

        When(x => x.CredentialsFile != null, () =>
        {
            RuleFor(x => x.CredentialsFile.Length)
                .LessThanOrEqualTo(MaxFileSizeBytes)
                .WithMessage($"File size must not exceed {MaxFileSizeBytes / 1024}KB");

            RuleFor(x => x.CredentialsFile.FileName)
                .Must(fileName => AllowedExtensions.Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                .WithMessage($"Only {string.Join(", ", AllowedExtensions)} files are allowed");

            RuleFor(x => x.CredentialsFile.ContentType)
                .Must(contentType => contentType == "application/json" || contentType == "text/json")
                .WithMessage("File must be a valid JSON file");
        });
    }
}
