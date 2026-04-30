---
name: api-contract-doc-sync
description: Manter sincronização entre contrato da API (payloads, respostas, status HTTP), implementação de endpoints e documentação textual usada em Swagger/OpenAPI. Usar quando Codex alterar DTOs, comandos/queries, validações, exemplos JSON ou descrições de endpoint e precisar garantir que documentação e código permaneçam consistentes.
---

# API Contract Doc Sync

## Objetivo

Evitar drift entre comportamento real dos endpoints e conteúdo de documentação exposto ao cliente.

## Fluxo Padrão

1. Identificar endpoints impactados pela mudança.
2. Atualizar comando/query/DTO primeiro.
3. Atualizar endpoint (`*Endpoints.cs`) com tipos de retorno e códigos HTTP.
4. Atualizar documentação (`*EndpointsDocumentation.cs`) com descrição e exemplos coerentes.
5. Executar checagem rápida com `scripts/check_endpoint_docs_sync.sh`.

## Checklist de Sincronia

- `Produces<T>` reflete tipo retornado real.
- `ProducesProblem` cobre falhas relevantes.
- Exemplo JSON possui campos válidos no contrato atual.
- Nomes de propriedades e tipos de canal estão atualizados.

## Referências

- Padrão de manutenção de contrato: [references/contract-sync-checklist.md](references/contract-sync-checklist.md)
- Guia de exemplos JSON: [references/json-example-guidelines.md](references/json-example-guidelines.md)

## Scripts

- `scripts/check_endpoint_docs_sync.sh`: aponta endpoints sem uso de documentação dedicada ou sem declarações de resposta.
