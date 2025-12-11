#!/bin/bash

# Script para criar e aplicar migrations no NotificationSystem
# Uso: ./migrate.sh <nome-da-migration>

set -e  # Sai em caso de erro

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Verifica se dotnet-ef está instalado
if ! command -v dotnet-ef &> /dev/null; then
    echo -e "${RED}Erro: dotnet-ef não está instalado${NC}"
    echo -e "${YELLOW}Instale com o comando:${NC}"
    echo -e "${GREEN}dotnet tool install --global dotnet-ef${NC}"
    echo ""
    exit 1
fi

# Verifica se o nome da migration foi fornecido
if [ -z "$1" ]; then
    echo -e "${RED}Erro: Nome da migration não fornecido${NC}"
    echo "Uso: ./migrate.sh <nome-da-migration>"
    echo "Exemplo: ./migrate.sh InitialCreate"
    exit 1
fi

MIGRATION_NAME=$1
API_PROJECT="src/NotificationSystem.Api"
INFRASTRUCTURE_PROJECT="src/NotificationSystem.Infrastructure"

echo -e "${YELLOW}========================================${NC}"
echo -e "${YELLOW}  Criando e Aplicando Migration${NC}"
echo -e "${YELLOW}========================================${NC}"
echo ""

# Verifica se os projetos existem
if [ ! -d "$API_PROJECT" ]; then
    echo -e "${RED}Erro: Projeto API não encontrado em $API_PROJECT${NC}"
    exit 1
fi

if [ ! -d "$INFRASTRUCTURE_PROJECT" ]; then
    echo -e "${RED}Erro: Projeto Infrastructure não encontrado em $INFRASTRUCTURE_PROJECT${NC}"
    exit 1
fi

# 1. Adicionar Migration
echo -e "${GREEN}[1/2] Criando migration '$MIGRATION_NAME'...${NC}"
dotnet ef migrations add "$MIGRATION_NAME" \
    --project "$INFRASTRUCTURE_PROJECT" \
    --startup-project "$API_PROJECT" \
    --context NotificationDbContext

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Migration criada com sucesso!${NC}"
    echo ""
else
    echo -e "${RED}✗ Erro ao criar migration${NC}"
    exit 1
fi

# 2. Aplicar Migration
echo -e "${GREEN}[2/2] Aplicando migration ao banco de dados...${NC}"
dotnet ef database update \
    --project "$INFRASTRUCTURE_PROJECT" \
    --startup-project "$API_PROJECT" \
    --context NotificationDbContext

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Migration aplicada com sucesso!${NC}"
    echo ""
else
    echo -e "${RED}✗ Erro ao aplicar migration${NC}"
    exit 1
fi

echo -e "${GREEN}========================================${NC}"
echo -e "${GREEN}  Migration concluída com sucesso!${NC}"
echo -e "${GREEN}========================================${NC}"
