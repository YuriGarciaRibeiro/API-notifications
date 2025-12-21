#!/bin/bash

# Script para criar uma nova migration
# Uso: ./scripts/add-migration.sh NomeDaMigration

set -e  # Para a execução se houver erro

# Verificar se o nome da migration foi fornecido
if [ -z "$1" ]; then
    echo "❌ Erro: Nome da migration não fornecido!"
    echo ""
    echo "Uso: ./scripts/add-migration.sh NomeDaMigration"
    echo ""
    echo "Exemplos:"
    echo "  ./scripts/add-migration.sh AddUserEmailColumn"
    echo "  ./scripts/add-migration.sh UpdateNotificationSchema"
    echo "  ./scripts/add-migration.sh AddIndexesToNotifications"
    echo ""
    exit 1
fi

MIGRATION_NAME=$1

echo "=========================================="
echo "  Criando Nova Migration: $MIGRATION_NAME"
echo "=========================================="
echo ""

echo "📦 Restaurando dependências..."
dotnet restore

echo ""
echo "🔨 Compilando projeto..."
dotnet build --no-restore

echo ""
echo "📝 Criando migration '$MIGRATION_NAME'..."
dotnet ef migrations add "$MIGRATION_NAME" \
    --project src/NotificationSystem.Infrastructure \
    --startup-project src/NotificationSystem.Api \
    --output-dir Migrations

echo ""
echo "=========================================="
echo "✅ Migration '$MIGRATION_NAME' criada com sucesso!"
echo "=========================================="
echo ""
echo "📂 Arquivos criados em: src/NotificationSystem.Infrastructure/Migrations/"
echo ""
echo "🔍 Próximos passos:"
echo "   1. Revise os arquivos da migration gerados"
echo "   2. Aplique a migration com: ./scripts/migrate.sh"
echo "   3. Ou reverta se necessário com: ./scripts/rollback-migration.sh"
echo ""
