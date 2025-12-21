#!/bin/bash

# Script para listar todas as migrations (aplicadas e pendentes)
# Uso: ./scripts/list-migrations.sh

set -e  # Para a execução se houver erro

echo "=========================================="
echo "  Lista de Migrations"
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
    echo "⚠️  Container PostgreSQL não está rodando"
    echo "   Mostrando apenas migrations locais..."
    echo ""
    echo "📂 Migrations no código:"
    ls -1 src/NotificationSystem.Infrastructure/Migrations/*.cs | grep -v "Designer\|Snapshot" | sed 's/.*\///' | sed 's/\.cs//'
    exit 0
fi

echo "📦 Restaurando dependências..."
dotnet restore > /dev/null 2>&1

echo ""
echo "📋 Migrations aplicadas no banco de dados:"
echo "----------------------------------------"
docker exec notifications-postgres psql -U postgres -d notifications -c "SELECT \"MigrationId\", \"ProductVersion\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\";" 2>/dev/null || echo "Nenhuma migration aplicada ainda"

echo ""
echo "📂 Migrations disponíveis no código:"
echo "----------------------------------------"
ls -1 src/NotificationSystem.Infrastructure/Migrations/*.cs 2>/dev/null | grep -v "Designer\|Snapshot" | sed 's/.*\///' | sed 's/\.cs//' || echo "Nenhuma migration encontrada"

echo ""
echo "🔍 Status detalhado:"
echo "----------------------------------------"
dotnet ef migrations list \
    --project src/NotificationSystem.Infrastructure \
    --startup-project src/NotificationSystem.Api \
    --no-build 2>/dev/null || echo "Não foi possível obter status detalhado"

echo ""
