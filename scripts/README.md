# Scripts de Gerenciamento

Este diretório contém scripts organizados por categoria para facilitar o gerenciamento do projeto.

## 📁 Estrutura

```
scripts/
├── database/          # Scripts de gerenciamento de banco de dados
│   ├── migrate.sh
│   ├── add-migration.sh
│   ├── rollback-migration.sh
│   ├── list-migrations.sh
│   └── reset-database.sh
└── README.md
```

---

## 🗄️ Database Scripts

Scripts para gerenciar migrations do Entity Framework Core.

### 1. `database/migrate.sh` - Aplicar Migrations

Aplica todas as migrations pendentes no banco de dados.

**Uso:**
```bash
./scripts/database/migrate.sh
```

**O que faz:**
- Verifica se Docker está rodando
- Verifica/inicia o container PostgreSQL
- Restaura dependências
- Compila o projeto
- Aplica as migrations
- Mostra as tabelas criadas

**Quando usar:**
- Após criar uma nova migration
- Ao clonar o projeto pela primeira vez
- Quando outro dev criar migrations e você fizer pull

---

### 2. `add-migration.sh` - Criar Nova Migration

Cria uma nova migration baseada nas mudanças do modelo.

**Uso:**
```bash
./scripts/database/add-migration.sh NomeDaMigration
```

**Exemplos:**
```bash
# Adicionar nova coluna
./scripts/database/add-migration.sh AddUserEmailColumn

# Atualizar schema
./scripts/database/add-migration.sh UpdateNotificationSchema

# Adicionar índices
./scripts/database/add-migration.sh AddIndexesToNotifications
```

**O que faz:**
- Restaura dependências
- Compila o projeto
- Cria uma nova migration com o nome fornecido
- Salva em `src/NotificationSystem.Infrastructure/Migrations/`

**Quando usar:**
- Após modificar entidades no Domain
- Após adicionar/remover propriedades
- Após criar novas entidades
- Após modificar configurações do EF Core

**⚠️ Importante:**
- Sempre revise os arquivos gerados antes de aplicar
- Nomes devem ser descritivos e em PascalCase
- Não edite migrations já aplicadas em produção

---

### 3. `rollback-migration.sh` - Reverter Migrations

Reverte migrations aplicadas no banco de dados.

**Uso:**
```bash
# Reverter TODAS as migrations (volta ao estado inicial)
./scripts/database/rollback-migration.sh

# Reverter até uma migration específica
./scripts/database/rollback-migration.sh NomeDaMigration
```

**Exemplos:**
```bash
# Desfazer última migration
./scripts/database/rollback-migration.sh

# Voltar para migration específica
./scripts/database/rollback-migration.sh InitialMigration
```

**O que faz:**
- Verifica se PostgreSQL está rodando
- Reverte as migrations no banco
- Remove os arquivos da migration (se reverter todas)
- Mostra histórico atualizado

**Quando usar:**
- Quando uma migration tem erro
- Para corrigir uma migration antes de commitar
- Em ambiente de desenvolvimento (NUNCA em produção!)

**⚠️ ATENÇÃO:**
- Pode causar perda de dados
- Use apenas em desenvolvimento
- Em produção, crie migration de correção ao invés de reverter

---

### 4. `list-migrations.sh` - Listar Migrations

Lista todas as migrations (aplicadas e pendentes).

**Uso:**
```bash
./scripts/database/list-migrations.sh
```

**O que mostra:**
- Migrations aplicadas no banco de dados
- Migrations disponíveis no código
- Status detalhado de cada migration

**Quando usar:**
- Para ver quais migrations já foram aplicadas
- Para debugar problemas de sincronização
- Antes de aplicar ou reverter migrations

---

### 5. `reset-database.sh` - Resetar Banco Completamente

**⚠️ PERIGO:** Apaga TODOS os dados e recria o banco do zero!

**Uso:**
```bash
./scripts/database/reset-database.sh
```

**O que faz:**
- Apaga o banco de dados completamente
- Remove todas as migrations aplicadas
- Recria o banco
- Aplica todas as migrations novamente

**Quando usar:**
- Quando o banco está em estado inconsistente
- Para limpar dados de teste
- Ao resetar ambiente de desenvolvimento
- **NUNCA em produção!**

**⚠️ ATENÇÃO:**
- Requer confirmação manual (digite 'SIM')
- Apaga TODOS os dados irreversivelmente
- Use com extremo cuidado

---

## 🔄 Workflow Comum

### Criar uma nova feature que altera o modelo

```bash
# 1. Modifique suas entidades no Domain
# (ex: adicione uma propriedade em Notification.cs)

# 2. Crie a migration
./scripts/database/add-migration.sh AddNewFeature

# 3. Revise os arquivos gerados
# src/NotificationSystem.Infrastructure/Migrations/

# 4. Aplique a migration
./scripts/database/migrate.sh

# 5. Teste suas mudanças
```

### Corrigir uma migration com erro

```bash
# 1. Reverter a migration problemática
./scripts/database/rollback-migration.sh

# 2. Corrigir o modelo/configuração

# 3. Criar nova migration
./scripts/database/add-migration.sh FixedMigration

# 4. Aplicar
./scripts/database/migrate.sh
```

### Sincronizar com mudanças de outros devs

```bash
# 1. Fazer pull do repositório
git pull

# 2. Verificar se há novas migrations
./scripts/database/list-migrations.sh

# 3. Aplicar migrations pendentes
./scripts/database/migrate.sh
```

### Resetar ambiente de desenvolvimento

```bash
# Resetar completamente
./scripts/database/reset-database.sh
```

---

## 🛠️ Troubleshooting

### "Docker não está rodando"
```bash
# Inicie o Docker Desktop e aguarde alguns segundos
```

### "Container PostgreSQL não encontrado"
```bash
# Suba os containers
docker-compose up -d

# Aguarde o PostgreSQL ficar pronto (5-10 segundos)
```

### "Build failed"
```bash
# Certifique-se de que o código compila
dotnet build

# Verifique erros de sintaxe nas entidades
```

### "Migration already applied"
```bash
# Liste as migrations
./scripts/database/list-migrations.sh

# Reverta se necessário
./scripts/database/rollback-migration.sh
```

### "Connection error"
```bash
# Verifique se PostgreSQL está saudável
docker ps

# Verifique a connection string
cat src/NotificationSystem.Api/appsettings.json | grep ConnectionString

# Teste a conexão diretamente
docker exec notifications-postgres psql -U postgres -d notifications -c "SELECT 1;"
```

---

## 📚 Recursos

- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [EF Core CLI Reference](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)

---

## 🔒 Boas Práticas

1. **Sempre revise migrations antes de aplicar**
   - Verifique os arquivos gerados
   - Certifique-se de que as mudanças fazem sentido

2. **Use nomes descritivos**
   - ✅ `AddUserEmailVerification`
   - ❌ `Migration1`, `Update`, `Fix`

3. **Teste localmente primeiro**
   - Aplique em dev/local
   - Teste a aplicação
   - Só então commite

4. **Nunca edite migrations aplicadas**
   - Se a migration já foi aplicada, crie uma nova
   - Não altere migrations que outros já aplicaram

5. **Mantenha migrations pequenas**
   - Uma migration = uma mudança lógica
   - Facilita rollback e debugging

6. **Backup antes de migrations em produção**
   - Sempre tenha backup do banco
   - Teste em staging primeiro

7. **Use transactions (padrão do EF)**
   - Migrations são transacionais por padrão
   - Em caso de erro, rollback automático

---

**Desenvolvido para o projeto NotificationSystem** 🚀
