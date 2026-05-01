# Ponto 4 (Media): quebra parcial do padrao CQRS/use-case em endpoints

## Problema

Parte da API segue muito bem o padrao `Endpoint -> MediatR -> UseCase`, mas existem excecoes:

1. `DeadLetterQueueEndpoints` chama servico diretamente.
2. `Provider upload endpoint` concentra parsing/validacao no endpoint.

## Impacto

- menor consistencia arquitetural;
- validacao espalhada;
- dificuldade maior para testes unitarios de regra;
- chance de bug por parsing manual no endpoint.

## Risco tecnico especifico

No upload de provider, `bool.Parse` em dados de form pode gerar exception para input malformado.

## Correcao recomendada

1. Criar use cases dedicados para operacoes de DLQ.
2. Mover parsing/validacao de upload para command+validator+handler.
3. Deixar endpoint com responsabilidade minima (binding + dispatch).

## Definicao de pronto

- endpoints sem regra de negocio relevante;
- validacoes centralizadas em validators/use cases;
- comportamento testavel via handler.

