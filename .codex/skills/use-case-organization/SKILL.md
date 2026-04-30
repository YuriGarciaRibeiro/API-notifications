---
name: use-case-organization
description: Padronizar organização de use cases em arquiteturas com Application Layer (ex.: CQRS/MediatR), definindo estrutura de pastas, convenções de nomes, responsabilidades por arquivo e checklist de consistência. Usar quando Codex precisar criar, refatorar ou revisar UseCases, Commands, Queries, Handlers e Validators em projetos .NET/C#.
---

# Use Case Organization

## Objetivo

Aplicar um padrão único para organizar use cases na camada de aplicação, facilitando manutenção, previsibilidade e revisão de código.

## Fluxo de Trabalho

1. Identificar se o caso é `Command` (escrita/mutação) ou `Query` (leitura).
2. Criar pasta do caso de uso em `UseCases/<NomeDoCaso>/`.
3. Criar arquivos mínimos conforme tipo do caso.
4. Garantir responsabilidades claras por arquivo (contrato, execução, validação, resposta).
5. Validar consistência com checklist final.

## Estrutura Padrão

Usar a estrutura abaixo para cada caso de uso:

```text
UseCases/
  <NomeDoCaso>/
    <NomeDoCaso>Command.cs      # Para escrita
    <NomeDoCaso>Query.cs        # Para leitura
    <NomeDoCaso>Handler.cs
    <NomeDoCaso>Validator.cs    # Se houver validação de entrada
    <NomeDoCaso>Response.cs     # Se retorno for composto
```

Aplicar apenas os arquivos necessários ao tipo do caso.

## Regras de Organização

- Manter um único propósito por pasta de use case.
- Nomear pasta e arquivos com o mesmo prefixo (`CreateUser`, `GetUserById`, `UpdateRole`).
- Colocar dependências externas (repositórios, serviços) apenas no `Handler`.
- Manter `Command`/`Query` como contrato de entrada, sem regra de negócio complexa.
- Colocar validações de entrada no `Validator`.
- Evitar lógica de domínio no `Validator`; delegar para domínio/serviços quando necessário.
- Retornar `Response` explícito quando houver múltiplos campos ou transformação.

## Responsabilidades por Arquivo

- `<NomeDoCaso>Command.cs` ou `<NomeDoCaso>Query.cs`: definir dados de entrada e intenção.
- `<NomeDoCaso>Handler.cs`: orquestrar fluxo, chamar domínio/repositório, persistir e retornar resultado.
- `<NomeDoCaso>Validator.cs`: validar formato, obrigatoriedade e regras sintáticas.
- `<NomeDoCaso>Response.cs`: modelar saída estável para API/camada chamadora.

## Checklist de Revisão

- Existe pasta própria para o use case.
- Nome da pasta e dos arquivos segue o mesmo prefixo.
- Existe separação entre contrato (`Command/Query`) e execução (`Handler`).
- Validações estão no `Validator` e não espalhadas no `Handler`.
- O `Handler` não contém regras de apresentação (HTTP, endpoint, UI).
- O retorno está claro (`Unit`, DTO simples ou `Response` específico).

## Referência Opcional

Para exemplos de variações e anti-padrões, consultar [references/use-case-patterns.md](references/use-case-patterns.md).
