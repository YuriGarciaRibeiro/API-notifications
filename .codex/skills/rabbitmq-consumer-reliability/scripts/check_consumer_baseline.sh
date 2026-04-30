#!/usr/bin/env bash
set -euo pipefail

if [[ $# -lt 1 ]]; then
  echo "Uso: $0 <path/Worker.cs>"
  exit 1
fi

worker="$1"

[[ -f "$worker" ]] || { echo "[ERRO] Arquivo nao encontrado: $worker"; exit 2; }

echo "Analisando $worker"

rg -n "RabbitMqConsumerBase<" "$worker" >/dev/null || { echo "[ERRO] Worker nao herda RabbitMqConsumerBase"; exit 3; }
rg -n "QueueName" "$worker" >/dev/null || { echo "[ERRO] QueueName nao definido"; exit 4; }
rg -n "ProcessMessageAsync" "$worker" >/dev/null || { echo "[ERRO] ProcessMessageAsync nao implementado"; exit 5; }
rg -n "GetChannelType" "$worker" >/dev/null || { echo "[ERRO] GetChannelType nao implementado"; exit 6; }

if rg -n "UpdateNotificationChannelStatusAsync" "$worker" >/dev/null; then
  echo "[OK] Atualizacao de status encontrada"
else
  echo "[ALERTA] Nao encontrou UpdateNotificationChannelStatusAsync no worker"
fi

echo "Baseline validado"
