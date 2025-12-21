#!/bin/bash

# Script para reverter a última migration ou voltar para uma migration específica
# Uso: ./scripts/rollback-migration.sh [NomeDaMigration]

set -e  # Para a execução se houver erro

echo "=========================================="
echo "  Revertendo Migrations"
echo "=========================================="
echo ""

# Verificar se o Docker está rodando
if ! docker ps &> /dev/null; then
    echo "❌ Erro: Docker não está rodando!"
    echo "   Por favor, inicie o Docker Desktop e tente novamente."
    exit 1
fi

# Verificar se o container do PostgreSQL está rodando
if ! docker ps --filter "name=notifications-postgres" --format "{{.Names}}" | grep -q "notifications-postgres"; then
    echo "❌ Erro: Container PostgreSQL não está rodando!"
    echo "   Execute: docker-compose up -d"
    exit 1
fi

echo "📦 Restaurando dependências..."
dotnet restore

echo ""
echo "🔨 Compilando projeto..."
dotnet build --no-restore

echo ""

# Se foi fornecido um nome de migration, reverter até ela
# Se não, reverter apenas a última
if [ -z "$1" ]; then
    echo "⏪ Revertendo última migration..."
    dotnet ef database update 0 \
        --project src/NotificationSystem.Infrastructure \
        --startup-project src/NotificationSystem.Api

    echo ""
    echo "🗑️  Removendo última migration..."
    dotnet ef migrations remove \
        --project src/NotificationSystem.Infrastructure \
        --startup-project src/NotificationSystem.Api \
        --force
else
    MIGRATION_NAME=$1
    echo "⏪ Revertendo até a migration: $MIGRATION_NAME..."
    dotnet ef database update "$MIGRATION_NAME" \
        --project src/NotificationSystem.Infrastructure \
        --startup-project src/NotificationSystem.Api
fi

echo ""
echo "=========================================="
echo "✅ Rollback concluído com sucesso!"
echo "=========================================="
echo ""
echo "📊 Estado atual do banco:"
docker exec notifications-postgres psql -U postgres -d notifications -c "SELECT * FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\";"
echo ""
