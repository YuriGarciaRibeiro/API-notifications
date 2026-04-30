---
name: provider-integration-pattern
description: Integrar ou substituir provedores de envio de notificações (Email, SMS, Push) mantendo padrão de factory, configuração dinâmica em banco, validação de ativação e dependências por interface. Usar quando Codex precisar adicionar novo provider, migrar provedor atual, corrigir integração externa ou padronizar fluxo de resolução de provider por canal.
---

# Provider Integration Pattern

## Objetivo

Adicionar integração de provider sem quebrar contratos existentes de envio e sem acoplamento direto em handlers/workers.

## Fluxo Padrão

1. Definir contrato da integração por interface na camada Application.
2. Implementar provider na Infrastructure.
3. Conectar provider ao factory/resolvedor existente.
4. Garantir validação de configuração ativa antes do envio.
5. Confirmar logs de provider selecionado e status de envio.
6. Validar pontos de integração com `scripts/check_provider_points.sh`.

## Checklist de Integração

- Usar injeção por interface, não por classe concreta.
- Centralizar seleção de provider no factory.
- Validar config ativa por `ChannelType` antes de enviar.
- Evitar branch de provider espalhada em múltiplos workers.
- Padronizar tratamento de erro para retry/DLQ.

## Referências

- Contratos e responsabilidades: [references/provider-contracts.md](references/provider-contracts.md)
- Rollout seguro de novo provider: [references/provider-rollout.md](references/provider-rollout.md)

## Scripts

- `scripts/check_provider_points.sh`: verifica pontos essenciais de integração no código.
