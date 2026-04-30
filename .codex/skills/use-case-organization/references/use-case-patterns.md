# Use Case Patterns

## Variações Comuns

### Command sem response explícito

Usar quando o retorno é apenas sucesso/falha.

Arquivos:
- `<Nome>Command.cs`
- `<Nome>Handler.cs`
- `<Nome>Validator.cs` (opcional)

### Query com response explícito

Usar quando a leitura retorna dados estruturados.

Arquivos:
- `<Nome>Query.cs`
- `<Nome>Handler.cs`
- `<Nome>Response.cs`
- `<Nome>Validator.cs` (opcional)

## Anti-padrões

- Misturar múltiplas intenções na mesma pasta de use case.
- Colocar acesso HTTP, headers ou status code dentro de `Handler`.
- Duplicar regras de validação em endpoint e `Validator` sem motivo.
- Criar `Response` para caso trivial de retorno simples.
- Fazer `Handler` depender de classes concretas em vez de interfaces da camada de aplicação.

## Naming rápido

- Escrita: `CreateX`, `UpdateX`, `DeleteX`, `AssignX`.
- Leitura: `GetX`, `ListX`, `SearchX`.
- Sempre alinhar nomes de pasta, arquivos e tipos C#.
