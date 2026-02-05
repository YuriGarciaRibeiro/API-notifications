# Guia de Permissões - Notification System API

**Última atualização**: 02/05/2026
**Total de Permissões**: 28

---

## 📋 Visão Geral

As permissões estão centralizadas em [`Permissions.cs`](src/NotificationSystem.Api/Authorization/Permissions.cs) e automaticamente registradas como políticas de autorização via extensão `AddPermissionPolicies()`.

### Benefícios:
✅ **Type-safe**: Sem risco de typos em strings
✅ **Centralizado**: Uma única fonte de verdade
✅ **Escalável**: Método `GetAll()` retorna todas as permissões
✅ **Intellisense**: Autocomplete nos endpoints

---

## 🔐 Permissões por Módulo

### 1️⃣ **ROLES** (4 permissões)
Gerenciamento de funções no sistema.

| Permissão | Descrição | Endpoint |
|-----------|-----------|----------|
| `role.create` | Criar nova role | `POST /api/roles` |
| `role.view` | Visualizar roles | `GET /api/roles` |
| `role.update` | Atualizar role | `PUT /api/roles/{id}` |
| `role.delete` | Deletar role | `DELETE /api/roles/{id}` |

---

### 2️⃣ **USERS** (6 permissões)
Gerenciamento de usuários.

| Permissão | Descrição | Endpoint |
|-----------|-----------|----------|
| `user.create` | Criar novo usuário | `POST /api/users` |
| `user.view` | Visualizar usuários | `GET /api/users` |
| `user.update` | Atualizar usuário | `PUT /api/users/{id}` |
| `user.delete` | Deletar usuário | `DELETE /api/users/{id}` |
| `user.change-password` | Alterar senha | `POST /api/users/{id}/change-password` |
| `user.assign-roles` | Atribuir roles | `POST /api/users/{id}/assign-roles` |

---

### 3️⃣ **NOTIFICATIONS** (4 permissões)
Gerenciamento de notificações.

| Permissão | Descrição | Endpoint |
|-----------|-----------|----------|
| `notification.create` | Criar notificação | `POST /api/notifications` |
| `notification.view` | Visualizar notificações | `GET /api/notifications` |
| `notification.stats` | Ver estatísticas | `GET /api/notifications/stats` |
| `notification.delete` | Deletar notificação | `DELETE /api/notifications/{id}` |

---

### 4️⃣ **PROVIDERS** (7 permissões) ⚙️ Admin
Gerenciamento de provedores de notificação (crítico).

| Permissão | Descrição | Endpoint |
|-----------|-----------|----------|
| `provider.create` | Criar provedor | `POST /api/admin/providers` |
| `provider.view` | Visualizar provedores | `GET /api/admin/providers` |
| `provider.upload` | Upload de credenciais | `POST /api/admin/providers/upload` |
| `provider.update` | Atualizar provedor | `PUT /api/admin/providers/{id}` |
| `provider.delete` | Deletar provedor | `DELETE /api/admin/providers/{id}` |
| `provider.toggle` | Ativar/desativar | `POST /api/admin/providers/{id}/toggle-active` |
| `provider.set-primary` | Definir como primário | `POST /api/admin/providers/{id}/set-primary` |

---

### 5️⃣ **DEAD LETTER QUEUE** (3 permissões) 🚨 Crítico
Gerenciamento de filas de mensagens com erro.

| Permissão | Descrição | Endpoint |
|-----------|-----------|----------|
| `dlq.view` | Ver DLQ stats/messages | `GET /api/dlq/stats`, `GET /api/dlq/{queueName}/messages` |
| `dlq.reprocess` | Reprocessar mensagens | `POST /api/dlq/{queueName}/reprocess/*` |
| `dlq.purge` | Limpar fila | `DELETE /api/dlq/{queueName}/purge` |

---

### 6️⃣ **PERMISSIONS** (1 permissão) 🔑 Admin
Gerenciamento de permissões.

| Permissão | Descrição | Endpoint |
|-----------|-----------|----------|
| `permission.view` | Listar permissões | `GET /api/roles/permissions` |

---

## 💡 Exemplos de Uso

### No Endpoint
```csharp
app.MapPost("/api/notifications", CreateNotification)
    .RequireAuthorization(Permissions.NotificationCreate);

app.MapDelete("/api/admin/providers/{id}", DeleteProvider)
    .RequireAuthorization(Permissions.ProviderDelete);
```

### No Handler/Serviço
```csharp
// Verificar permissão programaticamente
var authService = serviceProvider.GetRequiredService<IAuthService>();
var hasPermission = await authService.HasPermissionAsync(userId, Permissions.NotificationCreate);
```

### No JWT Token
```json
{
  "sub": "user-id",
  "email": "admin@example.com",
  "permission": ["role.create", "role.view", "user.create", "provider.view", "dlq.view"]
}
```

---

## 🎯 Recomendações de Roles

### Role: `Administrator` (Super Admin)
Acesso total ao sistema.
```csharp
Permissions.GetAll() // Todas as permissões
```

### Role: `Manager`
Gerencia usuários e notificações.
```csharp
new[] {
    Permissions.UserCreate,
    Permissions.UserView,
    Permissions.UserUpdate,
    Permissions.UserDelete,
    Permissions.UserAssignRoles,
    Permissions.NotificationCreate,
    Permissions.NotificationView,
    Permissions.NotificationStats,
    Permissions.RoleView
}
```

### Role: `Operator`
Monitora e gerencia DLQ.
```csharp
new[] {
    Permissions.DlqView,
    Permissions.DlqReprocess,
    Permissions.NotificationView,
    Permissions.NotificationStats
}
```

### Role: `Developer`
Gerencia provedores.
```csharp
new[] {
    Permissions.ProviderCreate,
    Permissions.ProviderView,
    Permissions.ProviderUpdate,
    Permissions.ProviderUpload,
    Permissions.ProviderToggle,
    Permissions.ProviderSetPrimary
}
```

---

## 🔄 Fluxo de Autorização

```
1. Request chega ao endpoint
    ↓
2. Middleware de autenticação valida JWT
    ↓
3. Extrai claims (incluindo "permission")
    ↓
4. Middleware de autorização compara com política
    ↓
5. Se tem permissão → executa handler
   Se não tem → retorna 403 Forbidden
```

---

## 📝 Checklist para Novas Permissões

Quando adicionar nova funcionalidade:

- [ ] Adicionar constante em `Permissions.cs`
- [ ] Adicionar em `GetAll()` method
- [ ] Adicionar no `.RequireAuthorization(Permissions.*)`
- [ ] Documentar aqui neste arquivo
- [ ] Atualizar roles recomendadas
- [ ] Testar com e sem permissão

---

## ⚠️ Notas Importantes

1. **Permissões críticas** (Provider, DLQ): Requerem `user.update` ou específicas
2. **change-password**: Usuário autenticado pode mudar sua própria senha
3. **assign-roles**: Só quem tem `user.assign-roles` pode atribuir roles
4. **DLQ purge**: Operação irreversível - use com cuidado!

---

## 🚀 Próximos Passos

- [ ] Implementar middleware de rate-limiting por permissão
- [ ] Adicionar audit log de ações por permissão
- [ ] Criar dashboard de permissões por usuário
- [ ] Implementar permissões baseadas em recurso (resource-based access control)
