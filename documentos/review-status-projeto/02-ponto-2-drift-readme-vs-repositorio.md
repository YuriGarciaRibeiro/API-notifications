# Ponto 2 (Alta): drift entre README e estado real do repositorio

## Problema

README descreve estrutura e prerequisitos que nao batem com o codigo atual.

## Divergencias principais

1. README cita `docs/` e `tests/`, mas essas pastas nao existem.
2. README cita `NotificationSystem.sln`, mas a solucao atual e `NotificationSystem.slnx`.
3. README diz `.NET SDK 8+`, enquanto projetos estao em `net10.0`.

## Impacto

- onboarding confuso;
- setup local falhando;
- perda de confianca na documentacao;
- ruido para novos contribuidores.

## Correcao recomendada

1. Atualizar estrutura do README para refletir pastas reais (`documentos/`, ausencia de `tests/`).
2. Ajustar comandos para `NotificationSystem.slnx`.
3. Atualizar prerequisito para `.NET 10 SDK`.
4. Marcar explicitamente o que esta planejado vs implementado.

## Definicao de pronto

- qualquer pessoa consegue clonar e subir projeto usando apenas README;
- nenhum caminho/comando quebrado;
- stack documentada igual ao csproj.

