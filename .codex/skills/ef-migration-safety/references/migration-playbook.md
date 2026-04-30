# Migration Playbook

## Desenvolvimento

- Criar migration.
- Revisar SQL implícito e impacto em dados.
- Aplicar localmente.
- Testar fluxo impactado.

## Homologação/Produção

- Revisar ordem de deploy (app e banco).
- Confirmar backup.
- Aplicar migration monitorando erros.
- Validar queries críticas após aplicação.

## Incidente

- Evitar rollback destrutivo em produção.
- Criar migration corretiva quando possível.
