#!/bin/bash

# Script para resetar completamente o banco de dados
# ATENÇÃO: Este script apaga TODOS os dados!
# Uso: ./scripts/reset-database.sh

set -e  # Para a execução se houver erro

echo "=========================================="
echo "  ⚠️  RESETAR BANCO DE DADOS"
echo "=========================================="
echo ""
echo "⚠️  ATENÇÃO: Este script irá:"
echo "   1. Apagar TODOS os dados do banco"
echo "   2. Remover todas as migrations aplicadas"
echo "   3. Recriar o banco do zero"
echo ""

read -p "Tem certeza que deseja continuar? (digite 'SIM' para confirmar): " confirmation

if [ "$confirmation" != "SIM" ]; then
    echo ""
    echo "❌ Operação cancelada."
    exit 0
fi

echo ""
echo "🗑️  Removendo banco de dados..."
dotnet ef database drop \
    --project src/NotificationSystem.Infrastructure \
    --startup-project src/NotificationSystem.Api \
    --force

echo ""
echo "📦 Restaurando dependências..."
dotnet restore

echo ""
echo "🔨 Compilando projeto..."
dotnet build --no-restore

echo ""
echo "🆕 Criando banco de dados e aplicando migrations..."
dotnet ef database update \
    --project src/NotificationSystem.Infrastructure \
    --startup-project src/NotificationSystem.Api

echo ""
echo "=========================================="
echo "✅ Banco de dados resetado com sucesso!"
echo "=========================================="
echo ""
echo "📊 Tabelas criadas:"
docker exec notifications-postgres psql -U postgres -d notifications -c "\dt"
echo ""
