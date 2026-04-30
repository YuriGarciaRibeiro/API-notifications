---
name: ef-migration-safety
description: Aplicar fluxo seguro de migrations EF Core no projeto de notificações, cobrindo criação, revisão, aplicação, rollback e validação de impacto entre ambientes. Usar quando Codex precisar alterar schema, gerar migration, corrigir migration defeituosa, sincronizar banco local ou reduzir risco operacional em mudanças de persistência.
---

# EF Migration Safety

## Objetivo

Executar mudanças de banco de forma previsível e auditável, priorizando segurança de dados e rollback controlado.

## Fluxo Padrão

1. Alterar entidades/configurações necessárias.
2. Criar migration com nome descritivo.
3. Revisar arquivos gerados antes de aplicar.
4. Aplicar migration localmente.
5. Validar comportamento da aplicação.
6. Planejar estratégia de produção sem rollback destrutivo.

## Comandos Preferenciais no Projeto

- `./scripts/database/add-migration.sh <NomeDaMigration>`
- `./scripts/database/migrate.sh`
- `./scripts/database/list-migrations.sh`
- `./scripts/database/rollback-migration.sh` (apenas dev)

## Regras de Segurança

- Não editar migration já aplicada em produção.
- Preferir migration corretiva em vez de rollback em produção.
- Confirmar backups e janela de manutenção para mudanças críticas.

## Referências

- Playbook por ambiente: [references/migration-playbook.md](references/migration-playbook.md)
- Convenções de naming/revisão: [references/migration-review-checklist.md](references/migration-review-checklist.md)

## Scripts

- `scripts/migration_preflight.sh`: checagem rápida antes de criar/aplicar migration.
