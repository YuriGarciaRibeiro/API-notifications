namespace NotificationSystem.Api.Endpoints;

public static class RoleEndpointsDocumentation
{
    public const string GetAllRolesDescription = @"
Lista todas as roles (papéis) cadastradas no sistema.

**Gerenciamento de Roles:**
Este endpoint retorna todas as roles disponíveis com suas permissões associadas.

**Autorização:**
Requer a permissão: **role.view**

**Sistema de Roles e Permissões:**
- Roles agrupam permissões relacionadas
- Cada role pode ter múltiplas permissões
- Usuários herdam permissões através de suas roles
- Roles do sistema não podem ser modificadas ou deletadas

**Response:**
Lista de roles contendo:
- **id**: ID único da role (GUID)
- **name**: Nome da role
- **description**: Descrição da role
- **isSystemRole**: Indica se é uma role do sistema (não pode ser deletada)
- **createdAt**: Data de criação
- **updatedAt**: Data da última atualização (nullable)
- **permissions**: Lista de permissões associadas

**Exemplo de Response:**
```json
[
  {
    ""id"": ""550e8400-e29b-41d4-a716-446655440000"",
    ""name"": ""Admin"",
    ""description"": ""Administrador com acesso total"",
    ""isSystemRole"": true,
    ""createdAt"": ""2024-01-01T10:00:00Z"",
    ""updatedAt"": null,
    ""permissions"": [
      {
        ""id"": ""660e8400-e29b-41d4-a716-446655440000"",
        ""code"": ""user.view"",
        ""name"": ""Visualizar Usuários"",
        ""description"": ""Permite visualizar lista de usuários"",
        ""category"": ""Users""
      },
      {
        ""id"": ""660e8400-e29b-41d4-a716-446655440001"",
        ""code"": ""user.create"",
        ""name"": ""Criar Usuários"",
        ""description"": ""Permite criar novos usuários"",
        ""category"": ""Users""
      }
    ]
  }
]
```

**Códigos de Status:**
- **200 OK**: Lista retornada com sucesso
- **403 Forbidden**: Sem permissão
- **500 Internal Server Error**: Erro no servidor
";

    public const string GetRoleByIdDescription = @"
Obtém os detalhes de uma role específica pelo ID.

**Gerenciamento de Roles:**
Retorna informações detalhadas de uma única role, incluindo todas as suas permissões.

**Autorização:**
Requer a permissão: **role.view**

**Parâmetros:**
- **id**: ID da role (GUID, obrigatório)

**Response:**
Informações completas da role com a mesma estrutura do GetAllRoles.

**Exemplo de Response:**
```json
{
  ""id"": ""550e8400-e29b-41d4-a716-446655440001"",
  ""name"": ""Manager"",
  ""description"": ""Gerente com permissões de gestão"",
  ""isSystemRole"": false,
  ""createdAt"": ""2024-01-05T14:30:00Z"",
  ""updatedAt"": ""2024-01-10T16:00:00Z"",
  ""permissions"": [
    {
      ""id"": ""660e8400-e29b-41d4-a716-446655440002"",
      ""code"": ""notification.view"",
      ""name"": ""Visualizar Notificações"",
      ""description"": ""Permite visualizar notificações"",
      ""category"": ""Notifications""
    }
  ]
}
```

**Códigos de Status:**
- **200 OK**: Role encontrada
- **404 Not Found**: Role não encontrada
- **403 Forbidden**: Sem permissão
- **500 Internal Server Error**: Erro no servidor
";

    public const string GetAllPermissionsDescription = @"
Lista todas as permissões disponíveis no sistema.

**Sistema de Permissões:**
Este endpoint retorna todas as permissões que podem ser atribuídas a roles.

**Autorização:**
Requer autenticação (qualquer usuário logado)

**Categorias de Permissões:**
As permissões são organizadas por categoria:
- **Users**: Gerenciamento de usuários (user.view, user.create, user.update, user.delete)
- **Roles**: Gerenciamento de roles (role.view, role.create, role.update, role.delete)
- **Notifications**: Gerenciamento de notificações
- **Reports**: Relatórios e estatísticas

**Response:**
Lista de permissões contendo:
- **id**: ID único da permissão (GUID)
- **code**: Código da permissão (usado nas policies)
- **name**: Nome legível da permissão
- **description**: Descrição do que a permissão permite
- **category**: Categoria da permissão

**Exemplo de Response:**
```json
[
  {
    ""id"": ""660e8400-e29b-41d4-a716-446655440000"",
    ""code"": ""user.view"",
    ""name"": ""Visualizar Usuários"",
    ""description"": ""Permite visualizar lista e detalhes de usuários"",
    ""category"": ""Users""
  },
  {
    ""id"": ""660e8400-e29b-41d4-a716-446655440001"",
    ""code"": ""user.create"",
    ""name"": ""Criar Usuários"",
    ""description"": ""Permite criar novos usuários no sistema"",
    ""category"": ""Users""
  },
  {
    ""id"": ""660e8400-e29b-41d4-a716-446655440002"",
    ""code"": ""role.view"",
    ""name"": ""Visualizar Roles"",
    ""description"": ""Permite visualizar roles e permissões"",
    ""category"": ""Roles""
  }
]
```

**Uso:**
Este endpoint é útil para:
- Construir interfaces de seleção de permissões
- Criar ou editar roles
- Documentar o sistema de permissões

**Códigos de Status:**
- **200 OK**: Lista retornada com sucesso
- **401 Unauthorized**: Não autenticado
- **500 Internal Server Error**: Erro no servidor
";

    public const string CreateRoleDescription = @"
Cria uma nova role no sistema.

**Criação de Roles:**
Permite criar roles customizadas com permissões específicas.

**Autorização:**
Requer a permissão: **role.create**

**Estrutura da Request:**
- **name**: Nome da role (obrigatório, único, mínimo 3 caracteres)
- **description**: Descrição da role (obrigatório)
- **permissionIds**: Array de IDs de permissões (opcional, pode ser vazio)

**Validações:**
- Nome deve ser único no sistema
- Role criada como não-sistema (isSystemRole = false)
- Permissões inválidas são ignoradas

**Exemplo de Request:**
```json
{
  ""name"": ""Support"",
  ""description"": ""Equipe de suporte ao cliente"",
  ""permissionIds"": [
    ""660e8400-e29b-41d4-a716-446655440002"",
    ""660e8400-e29b-41d4-a716-446655440003""
  ]
}
```

**Exemplo de Response:**
```json
{
  ""id"": ""550e8400-e29b-41d4-a716-446655440005"",
  ""name"": ""Support"",
  ""description"": ""Equipe de suporte ao cliente"",
  ""isSystemRole"": false,
  ""createdAt"": ""2024-01-15T11:30:00Z"",
  ""updatedAt"": null,
  ""permissions"": [
    {
      ""id"": ""660e8400-e29b-41d4-a716-446655440002"",
      ""code"": ""notification.view"",
      ""name"": ""Visualizar Notificações"",
      ""category"": ""Notifications""
    }
  ]
}
```

**Códigos de Status:**
- **201 Created**: Role criada com sucesso
- **400 Bad Request**: Dados inválidos ou nome já existe
- **403 Forbidden**: Sem permissão
- **500 Internal Server Error**: Erro no servidor
";

    public const string UpdateRoleDescription = @"
Atualiza uma role existente.

**Atualização de Roles:**
Permite modificar nome, descrição e permissões de uma role.

**Autorização:**
Requer a permissão: **role.update**

**Parâmetros:**
- **id**: ID da role (GUID, na URL)

**Estrutura da Request:**
Todos os campos são opcionais. Apenas os campos fornecidos serão atualizados:
- **name**: Novo nome (opcional, único, mínimo 3 caracteres)
- **description**: Nova descrição (opcional)
- **permissionIds**: Nova lista de permissões (opcional, array de GUIDs)

**Restrições:**
- Roles do sistema (isSystemRole = true) NÃO podem ser atualizadas
- Nome deve ser único se fornecido

**Exemplo de Request:**
```json
{
  ""name"": ""Customer Support"",
  ""description"": ""Equipe de suporte ao cliente - atualizado"",
  ""permissionIds"": [
    ""660e8400-e29b-41d4-a716-446655440002"",
    ""660e8400-e29b-41d4-a716-446655440003"",
    ""660e8400-e29b-41d4-a716-446655440004""
  ]
}
```

**Exemplo de Response:**
```json
{
  ""id"": ""550e8400-e29b-41d4-a716-446655440005"",
  ""name"": ""Customer Support"",
  ""description"": ""Equipe de suporte ao cliente - atualizado"",
  ""isSystemRole"": false,
  ""createdAt"": ""2024-01-15T11:30:00Z"",
  ""updatedAt"": ""2024-01-15T14:00:00Z"",
  ""permissions"": [...]
}
```

**Códigos de Status:**
- **200 OK**: Role atualizada com sucesso
- **400 Bad Request**: Dados inválidos ou tentativa de atualizar role do sistema
- **404 Not Found**: Role não encontrada
- **403 Forbidden**: Sem permissão
- **500 Internal Server Error**: Erro no servidor
";

    public const string DeleteRoleDescription = @"
Remove uma role do sistema.

**Exclusão de Roles:**
Remove permanentemente uma role do banco de dados.

**Autorização:**
Requer a permissão: **role.delete**

**Parâmetros:**
- **id**: ID da role a ser removida (GUID)

**Restrições:**
- Roles do sistema (isSystemRole = true) NÃO podem ser deletadas
- A exclusão é permanente e não pode ser desfeita
- Usuários que possuem esta role terão a role removida

**Importante:**
Antes de deletar uma role, considere:
- Quantos usuários possuem esta role
- Se a role está sendo usada em processos críticos
- Criar uma role de substituição se necessário

**Response de Sucesso:**
- **Status 204 No Content**: Role removida com sucesso (sem corpo)

**Códigos de Status:**
- **204 No Content**: Role removida
- **400 Bad Request**: Tentativa de deletar role do sistema
- **404 Not Found**: Role não encontrada
- **403 Forbidden**: Sem permissão
- **500 Internal Server Error**: Erro no servidor

**Exemplo de Fluxo:**
1. Verificar quantos usuários têm esta role: GET /api/users
2. Reatribuir usuários para outra role se necessário: POST /api/users/{id}/assign-roles
3. Deletar a role: DELETE /api/roles/{id}
";
}
