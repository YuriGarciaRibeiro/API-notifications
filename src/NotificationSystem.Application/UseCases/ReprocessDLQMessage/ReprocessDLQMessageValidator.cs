using FluentValidation;
using NotificationSystem.Application.Common.Mappings;

namespace NotificationSystem.Application.UseCases.ReprocessDLQMessage;

public class ReprocessDLQMessageValidator : AbstractValidator<ReprocessDLQMessageCommand>
{
    public ReprocessDLQMessageValidator()
    {
        RuleFor(x => x.QueueName)
            .NotEmpty()
            .WithMessage("Queue name is required")
            .Must(DeadLetterQueueMapping.IsValid)
            .WithMessage($"Invalid queue name. Valid queues: {string.Join(", ", DeadLetterQueueMapping.ValidQueues)}");

        RuleFor(x => x.DeliveryTag)
            .GreaterThan(0UL)
            .WithMessage("Delivery tag must be greater than 0");
    }
}
