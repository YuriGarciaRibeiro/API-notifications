#!/bin/bash

# Script para aplicar migrations no banco de dados
# Uso: ./scripts/migrate.sh

set -e  # Para a execução se houver erro

echo "=========================================="
echo "  Aplicando Migrations no Banco de Dados"
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
    echo "⚠️  Container PostgreSQL não encontrado. Subindo containers..."
    docker-compose up -d
    echo "⏳ Aguardando PostgreSQL ficar pronto..."
    sleep 8
fi

# Verificar se o PostgreSQL está saudável
POSTGRES_STATUS=$(docker ps --filter "name=notifications-postgres" --format "{{.Status}}")
if [[ $POSTGRES_STATUS == *"healthy"* ]] || [[ $POSTGRES_STATUS == *"Up"* ]]; then
    echo "✅ PostgreSQL está rodando"
else
    echo "⚠️  PostgreSQL não está saudável. Aguardando..."
    sleep 5
fi

echo ""
echo "📦 Restaurando dependências..."
dotnet restore

echo ""
echo "🔨 Compilando projeto..."
dotnet build --no-restore

echo ""
echo "🚀 Aplicando migrations..."
dotnet ef database update \
    --project src/NotificationSystem.Infrastructure \
    --startup-project src/NotificationSystem.Api

echo ""
echo "=========================================="
echo "✅ Migrations aplicadas com sucesso!"
echo "=========================================="
echo ""
echo "📊 Verificando tabelas criadas:"
docker exec notifications-postgres psql -U postgres -d notifications -c "\dt"
echo ""
