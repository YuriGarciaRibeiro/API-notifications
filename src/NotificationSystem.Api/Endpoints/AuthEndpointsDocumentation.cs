namespace NotificationSystem.Api.Endpoints;

public static class AuthEndpointsDocumentation
{
    public const string LoginDescription = @"
Autentica um usuário no sistema e retorna tokens de acesso e refresh.

**Autenticação JWT:**
Este endpoint implementa autenticação baseada em JWT (JSON Web Tokens) com suporte a refresh tokens.

**Estrutura da Request:**
- **email**: Email do usuário (obrigatório, deve ser um email válido)
- **password**: Senha do usuário (obrigatório, mínimo 6 caracteres)

**Fluxo de Autenticação:**
1. O sistema valida as credenciais do usuário
2. Verifica se a conta está ativa
3. Gera um Access Token JWT (curta duração)
4. Gera um Refresh Token (longa duração)
5. Armazena o Refresh Token no banco com informações de auditoria (IP, data)
6. Atualiza o campo LastLoginAt do usuário

**Response de Sucesso:**
- **accessToken**: Token JWT para autenticação de requisições (expira em 1 hora)
- **refreshToken**: Token para renovar o access token (expira em 7 dias)
- **expiresAt**: Data/hora de expiração do access token
- **user**: Informações do usuário autenticado
  - **id**: ID único do usuário
  - **email**: Email do usuário
  - **fullName**: Nome completo
  - **roles**: Lista de roles atribuídas ao usuário
  - **permissions**: Lista de permissões derivadas das roles

**Exemplo de Request:**
```json
{
  ""email"": ""admin@example.com"",
  ""password"": ""senha123""
}
```

**Exemplo de Response:**
```json
{
  ""accessToken"": ""eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."",
  ""refreshToken"": ""550e8400-e29b-41d4-a716-446655440000"",
  ""expiresAt"": ""2024-01-15T15:30:00Z"",
  ""user"": {
    ""id"": ""123e4567-e89b-12d3-a456-426614174000"",
    ""email"": ""admin@example.com"",
    ""fullName"": ""Administrador"",
    ""roles"": [""Admin""],
    ""permissions"": [""user.view"", ""user.create"", ""role.view""]
  }
}
```

**Códigos de Erro:**
- **400 Bad Request**: Credenciais inválidas ou conta inativa
- **500 Internal Server Error**: Erro no servidor
";

    public const string RegisterDescription = @"
Registra um novo usuário no sistema e retorna tokens de autenticação.

**Cadastro de Usuários:**
Este endpoint cria uma nova conta de usuário e autentica automaticamente o usuário após o registro.

**Estrutura da Request:**
- **email**: Email do usuário (obrigatório, único, deve ser válido)
- **password**: Senha (obrigatório, mínimo 6 caracteres)
- **fullName**: Nome completo (obrigatório, mínimo 3 caracteres)
- **roleIds**: Lista de IDs de roles a serem atribuídas (opcional, array de GUIDs)

**Validações:**
- Email deve ser único no sistema
- Senha é armazenada com hash BCrypt (não é reversível)
- O usuário é criado com status ativo por padrão

**Response de Sucesso:**
Similar ao endpoint de Login, retorna:
- **accessToken**: Token JWT para autenticação
- **refreshToken**: Token para renovação
- **expiresAt**: Data de expiração do access token
- **user**: Dados do usuário recém-criado

**Exemplo de Request:**
```json
{
  ""email"": ""novousuario@example.com"",
  ""password"": ""senha123"",
  ""fullName"": ""João da Silva"",
  ""roleIds"": [""550e8400-e29b-41d4-a716-446655440000""]
}
```

**Exemplo de Response:**
```json
{
  ""accessToken"": ""eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."",
  ""refreshToken"": ""550e8400-e29b-41d4-a716-446655440001"",
  ""expiresAt"": ""2024-01-15T15:30:00Z"",
  ""user"": {
    ""id"": ""123e4567-e89b-12d3-a456-426614174000"",
    ""email"": ""novousuario@example.com"",
    ""fullName"": ""João da Silva"",
    ""roles"": [""User""],
    ""permissions"": [""notification.view""]
  }
}
```

**Códigos de Erro:**
- **400 Bad Request**: Email já cadastrado ou dados inválidos
- **500 Internal Server Error**: Erro no servidor
";

    public const string RefreshTokenDescription = @"
Renova o access token usando um refresh token válido.

**Renovação de Tokens:**
Quando o access token expira, use este endpoint para obter um novo sem precisar fazer login novamente.

**Estrutura da Request:**
- **refreshToken**: Token de renovação obtido no login ou registro (obrigatório)

**Fluxo de Renovação:**
1. Valida se o refresh token existe e está ativo
2. Verifica se não foi revogado
3. Verifica se não expirou (7 dias de validade)
4. Valida o IP de origem (segurança adicional)
5. Gera um novo access token
6. Gera um novo refresh token (rotação de tokens)
7. Revoga o refresh token antigo

**Segurança - Rotação de Tokens:**
Por questões de segurança, cada renovação:
- Gera um novo refresh token
- Revoga o refresh token antigo
- Impede reutilização de tokens

**Exemplo de Request:**
```json
{
  ""refreshToken"": ""550e8400-e29b-41d4-a716-446655440000""
}
```

**Exemplo de Response:**
```json
{
  ""accessToken"": ""eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."",
  ""refreshToken"": ""550e8400-e29b-41d4-a716-446655440002"",
  ""expiresAt"": ""2024-01-15T16:30:00Z"",
  ""user"": {
    ""id"": ""123e4567-e89b-12d3-a456-426614174000"",
    ""email"": ""usuario@example.com"",
    ""fullName"": ""Usuário"",
    ""roles"": [""User""],
    ""permissions"": [""notification.view""]
  }
}
```

**Códigos de Erro:**
- **400 Bad Request**: Refresh token inválido, expirado ou revogado
- **500 Internal Server Error**: Erro no servidor
";

    public const string RevokeTokenDescription = @"
Revoga um refresh token, efetivamente realizando logout do usuário.

**Revogação de Tokens (Logout):**
Use este endpoint para invalidar um refresh token, impedindo que seja usado para renovar o access token.

**Estrutura da Request:**
- **refreshToken**: Token a ser revogado (obrigatório)

**Fluxo de Revogação:**
1. Localiza o refresh token no banco de dados
2. Valida o IP de origem
3. Marca o token como revogado
4. Registra a data/hora da revogação
5. Registra o IP de onde ocorreu a revogação

**Quando Usar:**
- Logout do usuário
- Logout de dispositivo específico
- Revogação de acesso por segurança

**Auditoria:**
Todas as revogações são registradas com:
- Data/hora da revogação
- IP de origem
- Status do token (Revoked)

**Exemplo de Request:**
```json
{
  ""refreshToken"": ""550e8400-e29b-41d4-a716-446655440000""
}
```

**Response de Sucesso:**
- **Status 204 No Content**: Token revogado com sucesso (sem corpo na resposta)

**Códigos de Erro:**
- **400 Bad Request**: Refresh token inválido ou não encontrado
- **500 Internal Server Error**: Erro no servidor

**Nota:**
Após a revogação, o refresh token não poderá mais ser usado. O usuário precisará fazer login novamente.
";
}
