using FluentValidation;
using NotificationSystem.Application.Common.Mappings;

namespace NotificationSystem.Application.UseCases.GetDLQMessages;

public class GetDLQMessagesValidator : AbstractValidator<GetDLQMessagesQuery>
{
    public GetDLQMessagesValidator()
    {
        RuleFor(x => x.QueueName)
            .NotEmpty()
            .WithMessage("Queue name is required")
            .Must(DeadLetterQueueMapping.IsValid)
            .WithMessage($"Invalid queue name. Valid queues: {string.Join(", ", DeadLetterQueueMapping.ValidQueues)}");

        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .WithMessage("Limit must be greater than 0");
    }
}
