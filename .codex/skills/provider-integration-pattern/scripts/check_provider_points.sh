#!/usr/bin/env bash
set -euo pipefail

root="${1:-src}"

echo "Verificando integracao de providers em $root"

rg -n "IEmailProviderFactory|ISmsProviderFactory|IPushProviderFactory|ProviderFactory" "$root" >/dev/null || {
  echo "[ALERTA] Nenhum factory de provider detectado"
}

rg -n "HasActiveConfigAsync" "$root" >/dev/null || {
  echo "[ALERTA] Nao encontrou validacao de provider ativo"
}

rg -n "CreateEmailProvider|CreateSmsProvider|CreatePushProvider" "$root" >/dev/null || {
  echo "[ALERTA] Nao encontrou criacao dinamica de provider"
}

echo "Verificacao concluida"
