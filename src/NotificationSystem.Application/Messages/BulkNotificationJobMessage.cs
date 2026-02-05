namespace NotificationSystem.Application.Messages;

/// <summary>
/// Mensagem publicada quando um bulk job é criado.
/// Consumer fetcha job do BD e processa items.
/// </summary>
public record BulkNotificationJobMessage(
    Guid JobId
);