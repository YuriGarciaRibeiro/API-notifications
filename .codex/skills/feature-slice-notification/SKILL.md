---
name: feature-slice-notification
description: Criar ou refatorar features ponta a ponta na API de notificações em .NET/C#, cobrindo UseCases (Command/Query + Handler + Validator), endpoint Minimal API, autorização por permission e documentação de contrato. Usar quando Codex precisar implementar um novo fluxo funcional completo, ajustar payloads/respostas sem quebrar o padrão da arquitetura, ou revisar consistência entre Application e Api.
---

# Feature Slice Notification

## Objetivo

Implementar uma feature completa com padrão consistente entre `NotificationSystem.Application` e `NotificationSystem.Api`.

## Fluxo Padrão

1. Definir se o caso é `Command` (mutação) ou `Query` (leitura).
2. Criar ou ajustar pasta em `src/NotificationSystem.Application/UseCases/<FeatureName>/`.
3. Garantir arquivos mínimos: `Command/Query`, `Handler`, `Validator` (quando aplicável) e `Response` (quando retorno composto).
4. Expor a operação em `src/NotificationSystem.Api/Endpoints/*Endpoints.cs`.
5. Aplicar autorização com `Permissions.*`.
6. Atualizar descrição e exemplos em `*EndpointsDocumentation.cs` quando houver impacto de contrato.
7. Executar validação rápida com `scripts/check_feature_slice.sh`.

## Checklist de Implementação

- Manter nomes alinhados: pasta, tipos C# e arquivo.
- Manter parsing/validação de entrada fora de endpoint.
- Manter regra de negócio no handler/serviços de aplicação.
- Retornar `Result`/`Result<T>` de forma consistente.
- Mapear status e erros de forma uniforme no endpoint com `ToIResult()`.

## Referências

- Padrões de organização por use case: `../use-case-organization/SKILL.md`
- Checklist detalhado: [references/feature-checklist.md](references/feature-checklist.md)
- Convenções de endpoint/doc: [references/endpoint-doc-pattern.md](references/endpoint-doc-pattern.md)

## Scripts

- `scripts/check_feature_slice.sh`: valida presença mínima de arquivos e mapeamento de endpoint por nome de feature.
