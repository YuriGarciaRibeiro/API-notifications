using NotificationSystem.Application.Contracts.DeadLetter;

namespace NotificationSystem.Application.UseCases.GetDLQMessages;

public record GetDLQMessagesResponse(IEnumerable<DeadLetterMessageDto> Messages);
