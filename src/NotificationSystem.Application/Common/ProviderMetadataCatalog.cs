using NotificationSystem.Application.UseCases.GetProviderMetadata;
using NotificationSystem.Domain.Entities;

namespace NotificationSystem.Application.Common;

public static class ProviderMetadataCatalog
{
    private const string MetadataVersion = "1";

    public static ProviderMetadataResponse Build()
    {
        return new ProviderMetadataResponse(
            Version: MetadataVersion,
            GeneratedAt: DateTime.UtcNow,
            Channels:
            [
                new ProviderChannelMetadataResponse(ChannelType.Email, "Email", [ProviderType.Smtp, ProviderType.SendGrid, ProviderType.AwsSes]),
                new ProviderChannelMetadataResponse(ChannelType.Sms, "SMS", [ProviderType.Twilio]),
                new ProviderChannelMetadataResponse(ChannelType.Push, "Push", [ProviderType.Firebase]),
            ],
            Providers:
            [
                new ProviderDefinitionMetadataResponse(
                    ProviderType.Smtp,
                    "smtp",
                    "SMTP",
                    ChannelType.Email,
                    SupportsUpload: false,
                    DocsUrl: "https://support.google.com/mail/answer/7126229",
                    Fields:
                    [
                        new ProviderFieldMetadataResponse("host", "Servidor SMTP", "text", true, false, "smtp.gmail.com", null, @"^[a-zA-Z0-9][a-zA-Z0-9\-_.]+[a-zA-Z0-9]$"),
                        new ProviderFieldMetadataResponse("port", "Porta", "number", true, false, "587", "587 para TLS, 465 para SSL", null, Min: 1, Max: 65535),
                        new ProviderFieldMetadataResponse("username", "Usuário", "text", true, false, "seu-email@gmail.com", "Geralmente é o endereço de email", @"^[^\s@]+@[^\s@]+\.[^\s@]+$"),
                        new ProviderFieldMetadataResponse("password", "Senha", "password", true, true, null, "Gmail: use App Password. Será criptografado ao salvar", null, Min: null, Max: null, MinLength: 8),
                        new ProviderFieldMetadataResponse("fromEmail", "Email Remetente", "text", true, false, "noreply@empresa.com", "Email que aparecerá como remetente", @"^[^\s@]+@[^\s@]+\.[^\s@]+$"),
                        new ProviderFieldMetadataResponse("fromName", "Nome Remetente", "text", true, false, "Sistema de Notificações", "Nome que aparecerá como remetente", null, Min: null, Max: null, MinLength: 3),
                        new ProviderFieldMetadataResponse("enableSsl", "Usar SSL/TLS", "checkbox", false, false, null, null)
                    ]),

                new ProviderDefinitionMetadataResponse(
                    ProviderType.SendGrid,
                    "sendGrid",
                    "SendGrid",
                    ChannelType.Email,
                    SupportsUpload: false,
                    DocsUrl: "https://app.sendgrid.com/",
                    Fields:
                    [
                        new ProviderFieldMetadataResponse("apiKey", "API Key", "password", true, true, "SG.xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", "Encontrado no dashboard do SendGrid, será criptografado no banco de dados", @"^SG\."),
                        new ProviderFieldMetadataResponse("fromEmail", "Email Remetente", "text", true, false, "noreply@empresa.com", "Email que aparecerá como remetente", @"^[^\s@]+@[^\s@]+\.[^\s@]+$"),
                        new ProviderFieldMetadataResponse("fromName", "Nome Remetente", "text", true, false, "Sistema de Notificações", "Nome que aparecerá como remetente", null, Min: null, Max: null, MinLength: 3),
                    ]),

                new ProviderDefinitionMetadataResponse(
                    ProviderType.AwsSes,
                    "awsSes",
                    "AWS SES",
                    ChannelType.Email,
                    SupportsUpload: false,
                    DocsUrl: "https://console.aws.amazon.com/ses/",
                    Fields:
                    [
                        new ProviderFieldMetadataResponse("region", "AWS Region", "text", true, false, "us-east-1", "Região do SES (ex.: us-east-1, sa-east-1)", @"^[a-z]{2}\-[a-z]+\-\d$"),
                        new ProviderFieldMetadataResponse("accessKeyId", "Access Key ID", "text", false, true, "AKIA...", "Opcional: informe junto com Secret Access Key. Se vazio, usa credenciais do ambiente/IAM role"),
                        new ProviderFieldMetadataResponse("secretAccessKey", "Secret Access Key", "password", false, true, null, "Opcional: informe junto com Access Key ID"),
                        new ProviderFieldMetadataResponse("sessionToken", "Session Token", "password", false, true, null, "Opcional: use com credenciais temporárias (STS)"),
                        new ProviderFieldMetadataResponse("fromEmail", "Email Remetente", "text", true, false, "noreply@empresa.com", "Identidade de remetente verificada no AWS SES", @"^[^\s@]+@[^\s@]+\.[^\s@]+$"),
                        new ProviderFieldMetadataResponse("fromName", "Nome Remetente", "text", false, false, "Sistema de Notificações", "Opcional: nome exibido para o destinatário"),
                    ]),

                new ProviderDefinitionMetadataResponse(
                    ProviderType.Twilio,
                    "twilio",
                    "Twilio",
                    ChannelType.Sms,
                    SupportsUpload: false,
                    DocsUrl: "https://console.twilio.com/",
                    Fields:
                    [
                        new ProviderFieldMetadataResponse("accountSid", "Account SID", "text", true, false, "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", "Encontrado no Dashboard do Twilio. Deve começar com \"AC\"", @"^AC[a-fA-F0-9]{32}$"),
                        new ProviderFieldMetadataResponse("authToken", "Auth Token", "password", true, true, null, "Será criptografado no banco de dados", null, Min: null, Max: null, MinLength: 32),
                        new ProviderFieldMetadataResponse("fromPhoneNumber", "Número de Origem", "text", true, false, "+5511999999999", "Formato E.164: +55 (Brasil) + 11 (DDD) + 999999999", @"^\+[1-9]\d{1,14}$"),
                    ]),

                new ProviderDefinitionMetadataResponse(
                    ProviderType.Firebase,
                    "firebase",
                    "Firebase",
                    ChannelType.Push,
                    SupportsUpload: true,
                    DocsUrl: "https://console.firebase.google.com/",
                    Fields:
                    [
                        new ProviderFieldMetadataResponse("credentialsJson", "Credenciais JSON do Service Account", "textarea", true, true, null, "Cole o conteúdo completo do arquivo JSON do Firebase ou faça upload do arquivo"),
                    ])
            ]);
    }
}
