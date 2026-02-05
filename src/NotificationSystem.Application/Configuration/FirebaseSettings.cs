namespace NotificationSystem.Application.Configuration;

/// <summary>
/// Configuração para o provedor Firebase Cloud Messaging
/// </summary>
public class FirebaseSettings
{
    /// <summary>
    /// Caminho para o arquivo de credenciais JSON do Firebase
    /// </summary>
    public string? CredentialsPath { get; set; }

    /// <summary>
    /// Conteúdo JSON das credenciais do Firebase (alternativa ao arquivo)
    /// </summary>
    public string? CredentialsJson { get; set; }

    /// <summary>
    /// ID do projeto Firebase
    /// </summary>
    public string ProjectId { get; set; } = string.Empty;
}
