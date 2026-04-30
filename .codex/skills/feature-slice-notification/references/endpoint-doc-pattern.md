# Endpoint + Documentation Pattern

## Padrão recomendado

1. Definir rota em `*Endpoints.cs`.
2. Definir resumo curto com `.WithSummary(...)`.
3. Usar descrição longa centralizada em `*EndpointsDocumentation.cs` para contratos complexos.
4. Declarar respostas de sucesso e erro com `Produces`/`ProducesProblem`.

## Quando atualizar documentação

- Alterar campos de request/response.
- Alterar regras de validação com impacto no cliente.
- Adicionar novo canal/provider/opção de envio.
