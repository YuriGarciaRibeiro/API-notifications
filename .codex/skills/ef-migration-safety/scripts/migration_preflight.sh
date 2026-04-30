#!/usr/bin/env bash
set -euo pipefail

echo "Preflight de migrations"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "[ERRO] dotnet nao encontrado"
  exit 1
fi

[[ -f NotificationSystem.slnx || -f NotificationSystem.sln ]] || {
  echo "[ERRO] Solucao .NET nao encontrada na raiz"
  exit 2
}

[[ -d src/NotificationSystem.Infrastructure/Migrations ]] || {
  echo "[ALERTA] Pasta de migrations nao encontrada"
}

echo "[OK] Ambiente minimo validado"
echo "Sugestao: executar ./scripts/database/list-migrations.sh antes de aplicar mudancas"
