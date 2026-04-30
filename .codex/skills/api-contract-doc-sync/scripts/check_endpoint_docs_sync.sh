#!/usr/bin/env bash
set -euo pipefail

endpoints_dir="src/NotificationSystem.Api/Endpoints"

[[ -d "$endpoints_dir" ]] || { echo "[ERRO] Pasta nao encontrada: $endpoints_dir"; exit 1; }

echo "Validando endpoints em $endpoints_dir"

missing_produces=0
while IFS= read -r file; do
  if ! rg -n "Produces\(|ProducesProblem\(" "$file" >/dev/null; then
    echo "[ALERTA] Sem Produces/ProducesProblem: $file"
    missing_produces=$((missing_produces+1))
  fi
done < <(find "$endpoints_dir" -maxdepth 1 -type f -name '*Endpoints.cs' ! -name '*Documentation.cs' | sort)

if (( missing_produces == 0 )); then
  echo "[OK] Todos os endpoints verificados possuem declaracoes de resposta"
fi

echo "Checagem concluida"
