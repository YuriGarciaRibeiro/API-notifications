namespace NotificationSystem.Api.Endpoints;

public static class DeadLetterQueueEndpointsDocumentation
{
    public const string GetStatsDescription = """
        Retorna estatísticas de todas as Dead Letter Queues do sistema.

        Para cada DLQ, você verá:
        - Nome da fila
        - Quantidade de mensagens pendentes
        - Quantidade de consumidores conectados

        Use este endpoint para monitorar a saúde do sistema e identificar problemas de processamento.
        """;

    public const string GetMessagesDescription = """
        Lista as mensagens presentes em uma Dead Letter Queue específica.

        Parâmetros:
        - queueName: Nome da DLQ (sms-notifications-dlq, email-notifications-dlq, push-notifications-dlq)
        - limit: Número máximo de mensagens a retornar (padrão: 100)

        IMPORTANTE: Este endpoint NÃO remove as mensagens da fila. Ele apenas as lê para visualização.
        As mensagens permanecem na DLQ até que você as reprocesse ou limpe a fila.

        Use este endpoint para investigar o conteúdo das mensagens que falharam antes de decidir
        se vai reprocessá-las ou descartá-las.
        """;

    public const string ReprocessMessageDescription = """
        Reprocessa uma mensagem específica da Dead Letter Queue.

        O que acontece:
        1. A mensagem é removida da DLQ
        2. O contador de retry é resetado para 0
        3. A mensagem é republicada na fila original
        4. Os consumers irão processar a mensagem novamente

        Use quando:
        - Você corrigiu o problema que causou a falha
        - Quer reprocessar apenas mensagens específicas
        - Está testando a correção gradualmente

        IMPORTANTE: Se o problema original não foi corrigido, a mensagem voltará para a DLQ
        após esgotar as tentativas de retry.
        """;

    public const string ReprocessAllDescription = """
        Reprocessa TODAS as mensagens de uma Dead Letter Queue.

        ⚠️ CUIDADO: Esta operação afeta todas as mensagens da DLQ.

        O que acontece:
        1. Todas as mensagens são removidas da DLQ
        2. Cada mensagem tem seu contador de retry resetado
        3. Todas são republicadas na fila original
        4. Os consumers processarão todas as mensagens novamente

        Use quando:
        - Um serviço externo estava fora do ar e voltou (ex: API de SMS)
        - Você fez deploy de uma correção que resolve o problema de todas as mensagens
        - Tem certeza de que o problema foi resolvido globalmente

        NÃO use quando:
        - O problema ainda existe (as mensagens voltarão para a DLQ)
        - Apenas algumas mensagens têm dados válidos
        - Está em dúvida se o problema foi corrigido
        """;

    public const string PurgeDescription = """
        Remove PERMANENTEMENTE todas as mensagens de uma Dead Letter Queue.

        ⚠️⚠️⚠️ ATENÇÃO: ESTA OPERAÇÃO É IRREVERSÍVEL! ⚠️⚠️⚠️

        Todas as mensagens serão deletadas e NÃO poderão ser recuperadas.

        Use APENAS quando:
        - As mensagens são inválidas e não devem ser processadas
        - Você já exportou/salvou as mensagens importantes
        - Tem certeza ABSOLUTA de que não precisa dessas mensagens

        Casos de uso:
        - Limpar mensagens de teste após desenvolvimento
        - Remover mensagens com dados inválidos que não podem ser corrigidos
        - Resetar o sistema após identificar que todas as mensagens são ruins

        RECOMENDAÇÃO: Sempre exporte as mensagens antes de purgar usando GET /messages.
        """;
}
