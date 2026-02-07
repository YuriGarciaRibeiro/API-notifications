namespace NotificationSystem.Application.Interfaces;

/// <summary>
/// Serviço responsável por agendar campanhas usando Hangfire
/// </summary>
public interface ICampaignSchedulerService
{
    /// <summary>
    /// Agenda uma campanha para execução futura (disparo único)
    /// </summary>
    public string ScheduleCampaign(Guid jobId, DateTime scheduledFor);

    /// <summary>
    /// Agenda uma campanha recorrente usando expressão CRON
    /// </summary>
    public void ScheduleRecurringCampaign(Guid jobId, string cronExpression, string? timeZone = null);

    /// <summary>
    /// Remove agendamento de uma campanha
    /// </summary>
    public void CancelScheduledCampaign(string hangfireJobId);

    /// <summary>
    /// Remove campanha recorrente
    /// </summary>
    public void RemoveRecurringCampaign(string recurringJobId);
}