# Contract Sync Checklist

## Ao alterar DTO/Command/Query

- Revisar endpoint que recebe ou retorna o tipo alterado.
- Revisar documentação textual e exemplos.
- Revisar códigos de status esperados.

## Ao alterar endpoint

- Garantir que `WithSummary` e `WithDescription` reflitam o comportamento.
- Garantir que filtros/paginação estejam descritos quando aplicável.
