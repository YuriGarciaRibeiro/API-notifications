# Feature Checklist

## Pré-implementação

- Confirmar o objetivo funcional e o payload esperado.
- Identificar se a operação pertence a `Notification`, `Provider`, `User`, `Role`, `Audit` ou `Bulk`.
- Confirmar se precisa de paginação/filtro.

## Application Layer

- Criar `UseCases/<FeatureName>/`.
- Definir `Command`/`Query` com apenas dados de entrada.
- Implementar `Handler` com dependências por interface.
- Implementar `Validator` com regras de forma e obrigatoriedade.
- Criar `Response` apenas quando necessário.

## API Layer

- Adicionar rota no endpoint adequado (`MapGet`, `MapPost`, `MapPut`, `MapDelete`).
- Aplicar `RequireAuthorization(Permissions.*)`.
- Definir `.Produces` e `.ProducesProblem` coerentes.
- Atualizar descrição e exemplos quando payload mudar.

## Revisão final

- Confirmar nomes consistentes.
- Confirmar ausência de lógica HTTP no handler.
- Confirmar ausência de lógica de negócio no endpoint.
- Rodar verificação rápida da skill.
