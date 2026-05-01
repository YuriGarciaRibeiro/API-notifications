using FluentValidation;
using NotificationSystem.Application.Common.Mappings;

namespace NotificationSystem.Application.UseCases.ReprocessAllDLQMessages;

public class ReprocessAllDLQMessagesValidator : AbstractValidator<ReprocessAllDLQMessagesCommand>
{
    public ReprocessAllDLQMessagesValidator()
    {
        RuleFor(x => x.QueueName)
            .NotEmpty()
            .WithMessage("Queue name is required")
            .Must(DeadLetterQueueMapping.IsValid)
            .WithMessage($"Invalid queue name. Valid queues: {string.Join(", ", DeadLetterQueueMapping.ValidQueues)}");
    }
}
