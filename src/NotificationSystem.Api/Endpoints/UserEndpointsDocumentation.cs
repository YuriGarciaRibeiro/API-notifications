namespace NotificationSystem.Api.Endpoints;

public static class UserEndpointsDocumentation
{
    public const string GetAllUsersDescription = @"
Lista todos os usuários cadastrados no sistema.

**Gerenciamento de Usuários:**
Este endpoint retorna uma lista completa de todos os usuários com suas informações básicas e roles atribuídas.

**Autorização:**
Requer a permissão: **user.view**

**Response:**
Lista de usuários contendo:
- **id**: ID único do usuário (GUID)
- **email**: Email do usuário
- **fullName**: Nome completo
- **isActive**: Status da conta (ativo/inativo)
- **createdAt**: Data de criação da conta
- **updatedAt**: Data da última atualização (nullable)
- **lastLoginAt**: Data do último login (nullable)
- **roles**: Lista de roles atribuídas ao usuário com suas permissões

**Exemplo de Response:**
```json
[
  {
    ""id"": ""123e4567-e89b-12d3-a456-426614174000"",
    ""email"": ""admin@example.com"",
    ""fullName"": ""Administrador"",
    ""isActive"": true,
    ""createdAt"": ""2024-01-01T10:00:00Z"",
    ""updatedAt"": ""2024-01-10T15:30:00Z"",
    ""lastLoginAt"": ""2024-01-15T08:45:00Z"",
    ""roles"": [
      {
        ""id"": ""550e8400-e29b-41d4-a716-446655440000"",
        ""name"": ""Admin"",
        ""description"": ""Administrador do sistema"",
        ""isSystemRole"": true,
        ""permissions"": [
          {
            ""id"": ""660e8400-e29b-41d4-a716-446655440000"",
            ""code"": ""user.view"",
            ""name"": ""Visualizar Usuários"",
            ""category"": ""Users""
          }
        ]
      }
    ]
  }
]
```

**Códigos de Status:**
- **200 OK**: Lista retornada com sucesso
- **403 Forbidden**: Usuário sem permissão
- **500 Internal Server Error**: Erro no servidor
";

    public const string GetUserByIdDescription = @"
Obtém os detalhes de um usuário específico pelo ID.

**Gerenciamento de Usuários:**
Retorna informações detalhadas de um único usuário, incluindo todas as suas roles e permissões.

**Autorização:**
Requer a permissão: **user.view**

**Parâmetros:**
- **id**: ID do usuário (GUID, obrigatório)

**Response:**
Informações completas do usuário com a mesma estrutura do GetAllUsers.

**Exemplo de Response:**
```json
{
  ""id"": ""123e4567-e89b-12d3-a456-426614174000"",
  ""email"": ""usuario@example.com"",
  ""fullName"": ""João da Silva"",
  ""isActive"": true,
  ""createdAt"": ""2024-01-01T10:00:00Z"",
  ""updatedAt"": null,
  ""lastLoginAt"": ""2024-01-15T08:45:00Z"",
  ""roles"": [
    {
      ""id"": ""550e8400-e29b-41d4-a716-446655440001"",
      ""name"": ""User"",
      ""description"": ""Usuário padrão"",
      ""isSystemRole"": false,
      ""permissions"": []
    }
  ]
}
```

**Códigos de Status:**
- **200 OK**: Usuário encontrado
- **404 Not Found**: Usuário não encontrado
- **403 Forbidden**: Sem permissão
- **500 Internal Server Error**: Erro no servidor
";

    public const string CreateUserDescription = @"
Cria um novo usuário no sistema.

**Criação de Usuários:**
Este endpoint permite criar novos usuários e atribuir roles iniciais.

**Autorização:**
Requer a permissão: **user.create**

**Estrutura da Request:**
- **email**: Email do usuário (obrigatório, único, válido)
- **password**: Senha (obrigatório, mínimo 6 caracteres)
- **fullName**: Nome completo (obrigatório, mínimo 3 caracteres)
- **roleIds**: Array de IDs de roles (opcional)

**Validações:**
- Email deve ser único no sistema
- Senha é armazenada com hash BCrypt
- Usuário criado com status ativo por padrão

**Exemplo de Request:**
```json
{
  ""email"": ""novousuario@example.com"",
  ""password"": ""senha123"",
  ""fullName"": ""Maria Santos"",
  ""roleIds"": [
    ""550e8400-e29b-41d4-a716-446655440001""
  ]
}
```

**Exemplo de Response:**
```json
{
  ""id"": ""123e4567-e89b-12d3-a456-426614174000"",
  ""email"": ""novousuario@example.com"",
  ""fullName"": ""Maria Santos"",
  ""isActive"": true,
  ""createdAt"": ""2024-01-15T10:30:00Z"",
  ""updatedAt"": null,
  ""lastLoginAt"": null,
  ""roles"": [
    {
      ""id"": ""550e8400-e29b-41d4-a716-446655440001"",
      ""name"": ""User"",
      ""description"": ""Usuário padrão""
    }
  ]
}
```

**Códigos de Status:**
- **201 Created**: Usuário criado com sucesso
- **400 Bad Request**: Dados inválidos ou email já existe
- **403 Forbidden**: Sem permissão
- **500 Internal Server Error**: Erro no servidor
";

    public const string UpdateUserDescription = @"
Atualiza as informações de um usuário existente.

**Atualização de Usuários:**
Permite modificar email, nome, status e roles de um usuário.

**Autorização:**
Requer a permissão: **user.update**

**Parâmetros:**
- **id**: ID do usuário (GUID, na URL)

**Estrutura da Request:**
Todos os campos são opcionais. Apenas os campos fornecidos serão atualizados:
- **fullName**: Novo nome completo (opcional, mínimo 3 caracteres)
- **email**: Novo email (opcional, deve ser único e válido)
- **isActive**: Ativar/desativar conta (opcional, boolean)
- **roleIds**: Nova lista de roles (opcional, array de GUIDs)

**Exemplo de Request:**
```json
{
  ""fullName"": ""Maria Santos Silva"",
  ""isActive"": true,
  ""roleIds"": [
    ""550e8400-e29b-41d4-a716-446655440001"",
    ""550e8400-e29b-41d4-a716-446655440002""
  ]
}
```

**Exemplo de Response:**
```json
{
  ""id"": ""123e4567-e89b-12d3-a456-426614174000"",
  ""email"": ""usuario@example.com"",
  ""fullName"": ""Maria Santos Silva"",
  ""isActive"": true,
  ""createdAt"": ""2024-01-01T10:00:00Z"",
  ""updatedAt"": ""2024-01-15T11:00:00Z"",
  ""lastLoginAt"": ""2024-01-15T08:45:00Z"",
  ""roles"": [...]
}
```

**Códigos de Status:**
- **200 OK**: Usuário atualizado com sucesso
- **400 Bad Request**: Dados inválidos
- **404 Not Found**: Usuário não encontrado
- **403 Forbidden**: Sem permissão
- **500 Internal Server Error**: Erro no servidor
";

    public const string DeleteUserDescription = @"
Remove um usuário do sistema.

**Exclusão de Usuários:**
Remove permanentemente um usuário do banco de dados.

**Autorização:**
Requer a permissão: **user.delete**

**Parâmetros:**
- **id**: ID do usuário a ser removido (GUID)

**Importante:**
- A exclusão é permanente e não pode ser desfeita
- Todos os refresh tokens do usuário são revogados
- Dados relacionados podem ser afetados (verificar constraints do banco)

**Response de Sucesso:**
- **Status 204 No Content**: Usuário removido com sucesso (sem corpo)

**Códigos de Status:**
- **204 No Content**: Usuário removido
- **404 Not Found**: Usuário não encontrado
- **403 Forbidden**: Sem permissão
- **500 Internal Server Error**: Erro no servidor
";

    public const string ChangePasswordDescription = @"
Altera a senha de um usuário.

**Alteração de Senha:**
Permite que um usuário altere sua própria senha fornecendo a senha atual.

**Autorização:**
Requer autenticação (qualquer usuário logado pode alterar sua própria senha)

**Parâmetros:**
- **id**: ID do usuário (GUID, na URL)

**Estrutura da Request:**
- **currentPassword**: Senha atual (obrigatório)
- **newPassword**: Nova senha (obrigatório, mínimo 6 caracteres)

**Validações:**
- A senha atual deve estar correta
- A nova senha deve atender aos requisitos mínimos
- A nova senha é armazenada com hash BCrypt

**Exemplo de Request:**
```json
{
  ""currentPassword"": ""senhaAntiga123"",
  ""newPassword"": ""novaSenha456""
}
```

**Response de Sucesso:**
- **Status 204 No Content**: Senha alterada com sucesso

**Códigos de Status:**
- **204 No Content**: Senha alterada
- **400 Bad Request**: Senha atual incorreta ou nova senha inválida
- **401 Unauthorized**: Não autenticado
- **500 Internal Server Error**: Erro no servidor
";

    public const string AssignRolesDescription = @"
Atribui roles a um usuário.

**Atribuição de Roles:**
Substitui todas as roles atuais do usuário pelas roles fornecidas.

**Autorização:**
Requer a permissão: **user.update**

**Parâmetros:**
- **id**: ID do usuário (GUID, na URL)

**Estrutura da Request:**
- **roleIds**: Array de IDs de roles (obrigatório, não vazio)

**Comportamento:**
- Remove todas as roles atuais do usuário
- Atribui as novas roles fornecidas
- As permissões do usuário são atualizadas automaticamente

**Exemplo de Request:**
```json
{
  ""roleIds"": [
    ""550e8400-e29b-41d4-a716-446655440001"",
    ""550e8400-e29b-41d4-a716-446655440002""
  ]
}
```

**Response de Sucesso:**
- **Status 204 No Content**: Roles atribuídas com sucesso

**Códigos de Status:**
- **204 No Content**: Roles atribuídas
- **400 Bad Request**: IDs de roles inválidos ou lista vazia
- **404 Not Found**: Usuário não encontrado
- **403 Forbidden**: Sem permissão
- **500 Internal Server Error**: Erro no servidor
";
}
