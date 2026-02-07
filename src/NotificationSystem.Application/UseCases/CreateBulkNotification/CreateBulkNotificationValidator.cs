using FluentValidation;
using System.Text.RegularExpressions;

namespace NotificationSystem.Application.UseCases.CreateBulkNotification;

public class CreateBulkNotificationValidator : AbstractValidator<CreateBulkNotificationCommand>
{
    public CreateBulkNotificationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome da campanha é obrigatório")
            .MaximumLength(200).WithMessage("Nome não pode exceder 200 caracteres");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Pelo menos um item deve ser informado")
            .Must(items => items.Count > 0).WithMessage("A lista de itens não pode estar vazia");

        RuleForEach(x => x.Items).SetValidator(new BulkNotificationItemValidator());

        // Validação de agendamento: não pode ter ScheduledFor e RecurringCron ao mesmo tempo
        RuleFor(x => x)
            .Must(x => !(x.ScheduledFor.HasValue && !string.IsNullOrEmpty(x.RecurringCron)))
            .WithMessage("Não é possível especificar ScheduledFor e RecurringCron ao mesmo tempo. Escolha apenas um.");

        // Se ScheduledFor informado, deve ser no futuro
        When(x => x.ScheduledFor.HasValue, () =>
        {
            RuleFor(x => x.ScheduledFor)
                .Must(date => date > DateTime.UtcNow)
                .WithMessage("ScheduledFor deve ser uma data futura");
        });

        // Se RecurringCron informado, deve ser válido
        When(x => !string.IsNullOrEmpty(x.RecurringCron), () =>
        {
            RuleFor(x => x.RecurringCron)
                .Must(BeValidCronExpression!)
                .WithMessage("CronExpression inválida. Use formato cron válido (ex: '0 9 * * 1' para toda segunda às 9h)");
        });

        // TimeZone deve ser válido se informado
        When(x => !string.IsNullOrEmpty(x.TimeZone), () =>
        {
            RuleFor(x => x.TimeZone)
                .Must(BeValidTimeZone!)
                .WithMessage("TimeZone inválido. Use um TimeZone válido (ex: 'America/Sao_Paulo', 'UTC')");
        });
    }

    private bool BeValidCronExpression(string cronExpression)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
            return false;

        // Validação básica de formato cron (5 ou 6 campos)
        // Formato: segundo(opcional) minuto hora dia mês dia-da-semana
        var cronPattern = @"^(\*|([0-9]|1[0-9]|2[0-9]|3[0-9]|4[0-9]|5[0-9])|\*\/([0-9]|1[0-9]|2[0-9]|3[0-9]|4[0-9]|5[0-9])) (\*|([0-9]|1[0-9]|2[0-3])|\*\/([0-9]|1[0-9]|2[0-3])) (\*|([1-9]|1[0-9]|2[0-9]|3[0-1])|\*\/([1-9]|1[0-9]|2[0-9]|3[0-1])) (\*|([1-9]|1[0-2])|\*\/([1-9]|1[0-2])) (\*|([0-6])|\*\/([0-6]))$";

        return Regex.IsMatch(cronExpression, cronPattern);
    }

    private bool BeValidTimeZone(string timeZone)
    {
        try
        {
            TimeZoneInfo.FindSystemTimeZoneById(timeZone);
            return true;
        }
        catch
        {
            return false;
        }
    }
}

public class BulkNotificationItemValidator : AbstractValidator<CreateBulkNotificationItemRequest>
{
    public BulkNotificationItemValidator()
    {
        RuleFor(x => x.Recipient)
            .NotEmpty().WithMessage("Destinatário é obrigatório");

        RuleFor(x => x.Channel)
            .IsInEnum().WithMessage("Canal inválido");

        // Validações específicas por canal
        When(x => x.Channel == ChannelType.Email, () =>
        {
            RuleFor(x => x.Recipient)
                .EmailAddress().WithMessage("Email inválido");
        });

        When(x => x.Channel == ChannelType.Sms, () =>
        {
            RuleFor(x => x.Recipient)
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Número de telefone inválido");
        });
    }
}
