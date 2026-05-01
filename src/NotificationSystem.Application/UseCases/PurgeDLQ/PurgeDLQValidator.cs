using FluentValidation;
using NotificationSystem.Application.Common.Mappings;

namespace NotificationSystem.Application.UseCases.PurgeDLQ;

public class PurgeDLQValidator : AbstractValidator<PurgeDLQCommand>
{
    public PurgeDLQValidator()
    {
        RuleFor(x => x.QueueName)
            .NotEmpty()
            .WithMessage("Queue name is required")
            .Must(DeadLetterQueueMapping.IsValid)
            .WithMessage($"Invalid queue name. Valid queues: {string.Join(", ", DeadLetterQueueMapping.ValidQueues)}");
    }
}
