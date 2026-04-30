#!/usr/bin/env bash
set -euo pipefail

readme="README.md"
[[ -f "$readme" ]] || { echo "[ERRO] README.md nao encontrado"; exit 1; }

check_path() {
  local path="$1"
  if [[ -e "$path" ]]; then
    echo "[OK] $path"
  else
    echo "[DRIFT] Ausente: $path"
  fi
}

echo "Verificando caminhos comuns citados em README"
check_path "src"
check_path "scripts"
check_path "docs"
check_path "tests"
check_path "src/NotificationSystem.Application"
check_path "src/NotificationSystem.Infrastructure"
check_path "src/NotificationSystem.Api"

echo "Checagem concluida"
