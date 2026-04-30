---
name: repo-reality-check
description: Auditar aderência entre documentação do repositório (README/docs) e estado real do código, estrutura de pastas, scripts e fluxos executáveis. Usar quando Codex precisar validar onboarding técnico, preparar release, revisar documentação após refatorações ou identificar discrepâncias entre o que está escrito e o que realmente existe no projeto.
---

# Repo Reality Check

## Objetivo

Detectar divergências entre documentação e implementação para reduzir ruído em onboarding, manutenção e operação.

## Fluxo Padrão

1. Levantar afirmações estruturais do README (pastas, comandos, arquitetura).
2. Verificar existência real de pastas e arquivos mencionados.
3. Verificar comandos críticos citados na documentação.
4. Classificar divergências por severidade (bloqueante, relevante, cosmética).
5. Propor correções de documentação ou código.

## Checklist de Auditoria

- Pastas documentadas existem?
- Scripts citados existem e são executáveis?
- Nomes de projetos/camadas batem com a estrutura atual?
- Exemplos de comando continuam válidos?

## Referências

- Critérios de severidade: [references/drift-severity.md](references/drift-severity.md)
- Template de relatório: [references/reality-report-template.md](references/reality-report-template.md)

## Scripts

- `scripts/repo_reality_check.sh`: verifica rapidamente existência de diretórios-chave citados no README.
