# Ponto 3 (Media): exemplo de front no README desalinhado com contrato atual

## Resumo

O backend atual expoe `NotificationDto` com `Channels[]` polimorficos (`EmailChannelDto`, `SmsChannelDto`, `PushChannelDto`).  
O exemplo do README sugere outra leitura de contrato (tipos e shape diferentes), o que pode induzir frontend a implementar parsing errado.

---

## Onde esta o desalinhamento

## No README

- Exemplo TS usa `EmailNotificationDto`.
- Switch em `notification.type` no nivel da notificacao.

## No backend atual

- DTO principal: `NotificationDto`.
- Canais dentro de `notification.Channels`.
- Polimorfismo no `ChannelDto` com discriminador `type`.

---

## Risco pratico para o frontend

- frontend pode tentar ler campos de email/sms/push direto na notificacao;
- parsing de uniao discriminada fica errado;
- erros de tipagem ou bugs silenciosos no render.

---

## O que deve ser documentado corretamente

1. `NotificationDto` contem metadados gerais da notificacao.
2. Cada item em `Channels[]` tem `type` (`Email`/`Sms`/`Push`) + campos especificos.
3. Exemplo TS precisa iterar canais, nao assumir que notificacao e canal.

---

## Exemplo de contrato esperado (conceitual)

```json
{
  "id": "notification-id",
  "userId": "user-id",
  "createdAt": "2026-01-01T10:00:00Z",
  "origin": "User",
  "type": "Unique",
  "channels": [
    { "type": "Email", "to": "x@y.com", "subject": "..." },
    { "type": "Sms", "to": "+55...", "message": "..." }
  ]
}
```

---

## Plano de correcao no README

1. Atualizar exemplo TS para os nomes reais dos DTOs gerados.
2. Mostrar parsing por `notification.channels`.
3. Incluir observacao de que o discriminador `type` esta no canal.
4. Se possivel, gerar exemplo diretamente do Swagger real para reduzir drift futuro.

---

## Checklist de validacao

- exemplo compila em TypeScript;
- nomes usados no exemplo existem no client gerado;
- shape do JSON bate com resposta real da API;
- dev frontend consegue implementar UI sem adivinhacao.

---

## Definicao de pronto para o ponto 3

- README de integracao front reflete exatamente o contrato atual da API;
- exemplos TS estao consistentes com OpenAPI gerado.

