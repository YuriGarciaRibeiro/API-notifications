#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 1 ]]; then
  echo "Uso: $0 <FeatureName>"
  echo "Exemplo: $0 CreateNotification"
  exit 1
fi

feature="$1"
base="src/NotificationSystem.Application/UseCases/$feature"

if [[ ! -d "$base" ]]; then
  echo "[ERRO] Pasta de use case nao encontrada: $base"
  exit 2
fi

echo "[OK] Pasta encontrada: $base"

ls "$base" | rg -n "${feature}(Command|Query|Handler|Validator|Response)\\.cs" >/dev/null || {
  echo "[ERRO] Nenhum arquivo padrao encontrado para $feature"
  exit 3
}

echo "[OK] Arquivos padrao detectados em $base"

if rg -n "$feature|${feature}Command|${feature}Query" src/NotificationSystem.Api/Endpoints -g '*Endpoints.cs' >/dev/null; then
  echo "[OK] Referencia ao feature encontrada em Endpoints"
else
  echo "[ALERTA] Nenhuma referencia ao feature encontrada em Endpoints"
fi

echo "Verificacao concluida"
