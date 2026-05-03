# Ponto 6: analise completa de diferencas entre backend e frontend

## Escopo analisado

- Backend: `API-notifications` (este repositorio).
- Frontend: `../notification-hub` (workspace VSCode).

## Evidencias de que os projetos se conectam

1. Front usa `VITE_API_BASE_URL=http://localhost:5235`.
2. Backend sobe em `http://localhost:5235` (`launchSettings.json`).
3. Front consome rotas reais de auth, notifications, bulk, providers, users, audit e dlq.

---

## Diferenca estrutural principal

1. Backend e um monolito .NET (Api/Application/Domain/Infrastructure + consumers RabbitMQ).
2. Frontend e um SPA React/Vite separado, em outro repositorio.
3. Integracao acontece entre dois repos no mesmo workspace.

---

## Cobertura funcional (status atual)

Dominios com cobertura backend + frontend:

1. Auth: login/register/refresh/revoke.
2. Users: CRUD, change-password, assign-roles.
3. Roles/Permissions: CRUD + listagem de permissoes.
4. Notifications: create/list/detail/stats.
5. Bulk: create/list/detail/progress/items/cancel + realtime SignalR.
6. Providers: list/create/upload/update config segura/set-primary/toggle-active/delete/test-connection.
7. DLQ: stats/list/reprocess/reprocess-all/purge.
8. Audit logs: list/detail/entity history.

---

## Ajustes implementados nesta rodada final

1. **Contrato e Settings seguros**
- `GET /api/admin/providers/{id}/configuration` (sem segredos em claro).
- `PUT /api/admin/providers/{id}` (update parcial preservando segredo omitido/vazio).
- `POST /api/admin/providers/{id}/test-connection` (smoke test sem envio real).

2. **Realtime de bulk com SignalR**
- Hub: `GET/WS /hubs/bulk-progress` autenticado por JWT.
- Subscription por `jobId` (grupos) + broadcast de progresso padronizado.
- Front `BulkJobDetails` conectado com reconexao automatica e fallback para polling.

3. **Cobertura de endpoints antes faltantes**
- `POST /api/users/{id}/assign-roles`
- `GET /api/audit-logs/entity/{entityName}/{entityId}`
- `POST /api/admin/providers/upload`

4. **Estrategia sem geracao automatica**
- Mantido contrato manual no frontend.
- Adicionado teste de contrato manual (`scripts/contract-tests.mjs`) para validar runtime.
- Workflow CI de contrato no frontend (`.github/workflows/contract-tests.yml`).

---

## Diferencas que continuam por decisao de arquitetura

1. **Sem codegen OpenAPI no frontend**
- Tipos e services continuam manuais.
- Mitigacao de drift: testes de contrato executados no pipeline.

2. **Processamento bulk assíncrono permanece nos consumers**
- API nao processa envio em si; apenas orquestra e expoe estado.

---

## Conclusao objetiva

As pendencias principais do ponto 6 foram fechadas: Settings operacional com seguranca de segredo, realtime de bulk via SignalR, endpoint de test connection, e estrategia de contrato manual protegida por testes de contrato no CI.
