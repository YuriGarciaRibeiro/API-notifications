# Go - Guia de Referência Rápida

> **Consulta Rápida**: Use Ctrl+F para buscar conceitos específicos
>
> Este é seu "dicionário" de Go. Sempre que tiver dúvida de sintaxe ou como fazer algo, consulte aqui.

---

## 📑 Índice Rápido

- [Comandos CLI](#comandos-cli)
- [Sintaxe Básica](#sintaxe-básica)
- [Tipos de Dados](#tipos-de-dados)
- [Variáveis e Constantes](#variáveis-e-constantes)
- [Funções](#funções)
- [Structs](#structs)
- [Ponteiros](#ponteiros)
- [Arrays, Slices e Maps](#arrays-slices-e-maps)
- [Controle de Fluxo](#controle-de-fluxo)
- [Error Handling](#error-handling)
- [Interfaces](#interfaces)
- [Goroutines e Channels](#goroutines-e-channels)
- [Packages e Imports](#packages-e-imports)
- [JSON](#json)
- [Testes](#testes)
- [Dicas e Truques](#dicas-e-truques)

---

## Comandos CLI

### Comandos Essenciais

```bash
# Inicializar módulo
go mod init github.com/usuario/projeto

# Instalar dependência
go get github.com/gin-gonic/gin
go get -u github.com/gin-gonic/gin  # atualizar para última versão

# Baixar todas as dependências
go mod download

# Limpar dependências não usadas
go mod tidy

# Rodar programa
go run main.go
go run cmd/api/main.go  # com caminho

# Compilar (gera executável)
go build                  # gera executável com nome do diretório
go build -o meuprograma   # gera executável com nome específico
go build ./cmd/api        # compila pacote específico

# Rodar testes
go test                   # testa pacote atual
go test ./...             # testa todos os pacotes
go test -v                # verbose (mostra todos os testes)
go test -cover            # com cobertura
go test -run TestNome     # roda teste específico

# Formatar código (sempre use!)
go fmt ./...              # formata todos os arquivos
gofmt -w arquivo.go       # formata arquivo específico

# Ver documentação
go doc fmt.Println
go doc -all encoding/json

# Verificar erros comuns
go vet ./...

# Instalar ferramentas
go install github.com/cosmtrek/air@latest
```

### Comandos de Debug

```bash
# Race detector (detecta condições de corrida)
go run -race main.go
go test -race ./...

# Build tags
go build -tags=integration
go test -tags=integration ./...

# Ver dependências
go list -m all              # lista todas
go mod graph                # mostra grafo de dependências
go mod why github.com/gin   # por que essa dependência existe

# Limpar cache
go clean -cache
go clean -modcache
```

---

## Sintaxe Básica

### Hello World

```go
package main

import "fmt"

func main() {
    fmt.Println("Hello, World!")
}
```

### Estrutura de Arquivo Go

```go
// 1. Package declaration (sempre primeira linha)
package main

// 2. Imports
import (
    "fmt"
    "time"

    "github.com/gin-gonic/gin"  // pacote externo
)

// 3. Constantes
const MaxRetries = 3

// 4. Variáveis globais (evite quando possível)
var Logger *zap.Logger

// 5. Tipos customizados
type User struct {
    Name string
    Age  int
}

// 6. Funções
func main() {
    // código aqui
}
```

### Comentários

```go
// Comentário de linha única

/*
Comentário
de múltiplas
linhas
*/

// Comentário de documentação (aparece no go doc)
// Add soma dois números e retorna o resultado
func Add(a, b int) int {
    return a + b
}
```

---

## Tipos de Dados

### Tipos Básicos

```go
// Inteiros
var i int = 42              // tamanho depende da arquitetura (32 ou 64 bits)
var i8 int8 = 127           // -128 a 127
var i16 int16 = 32767
var i32 int32 = 2147483647
var i64 int64 = 9223372036854775807

var u uint = 42             // unsigned (só positivos)
var u8 uint8 = 255          // 0 a 255
var u16 uint16 = 65535
var u32 uint32 = 4294967295
var u64 uint64 = 18446744073709551615

// Floats
var f32 float32 = 3.14
var f64 float64 = 3.14159265359

// String
var s string = "Hello"
var s2 string = `
    String com
    múltiplas linhas
    (raw string literal)
`

// Boolean
var b bool = true

// Byte (alias para uint8)
var by byte = 'A'  // 65

// Rune (alias para int32, representa um caractere Unicode)
var r rune = '世'

// Complex
var c complex64 = 1 + 2i
var c2 complex128 = 1 + 2i
```

### Zero Values (valores padrão)

```go
var i int        // 0
var f float64    // 0.0
var b bool       // false
var s string     // "" (string vazia)
var p *int       // nil
var sl []int     // nil
var m map[string]int  // nil
```

### Type Conversion

```go
var i int = 42
var f float64 = float64(i)  // conversão explícita
var u uint = uint(f)

// String conversions
import "strconv"

// int to string
s := strconv.Itoa(42)                    // "42"
s := fmt.Sprintf("%d", 42)               // "42"

// string to int
i, err := strconv.Atoi("42")             // 42, nil
i, err := strconv.ParseInt("42", 10, 64) // base 10, 64 bits

// float to string
s := fmt.Sprintf("%.2f", 3.14159)        // "3.14"

// string to float
f, err := strconv.ParseFloat("3.14", 64)
```

---

## Variáveis e Constantes

### Declaração de Variáveis

```go
// Forma explícita
var nome string = "João"
var idade int = 30

// Inferência de tipo
var nome = "João"     // Go infere que é string
var idade = 30        // Go infere que é int

// Short declaration (só dentro de funções!)
nome := "João"
idade := 30

// Múltiplas variáveis
var x, y int = 1, 2
a, b := 3, 4
var (
    name string = "João"
    age  int    = 30
)

// Declaração sem inicialização (recebe zero value)
var count int
var message string
```

### Constantes

```go
// Constante simples
const Pi = 3.14

// Múltiplas constantes
const (
    StatusOK       = 200
    StatusNotFound = 404
    StatusError    = 500
)

// iota (enumeração automática)
const (
    Sunday    = iota  // 0
    Monday            // 1
    Tuesday           // 2
    Wednesday         // 3
)

const (
    _  = iota         // ignora 0
    KB = 1 << (10 * iota)  // 1024
    MB                      // 1048576
    GB                      // 1073741824
)
```

---

## Funções

### Declaração Básica

```go
// Função simples
func sayHello() {
    fmt.Println("Hello!")
}

// Com parâmetros
func add(a int, b int) int {
    return a + b
}

// Parâmetros do mesmo tipo (sintaxe curta)
func add(a, b int) int {
    return a + b
}

// Múltiplos retornos
func divide(a, b float64) (float64, error) {
    if b == 0 {
        return 0, fmt.Errorf("divisão por zero")
    }
    return a / b, nil
}

// Retorno nomeado
func getUser() (name string, age int) {
    name = "João"
    age = 30
    return  // retorna name e age automaticamente
}

// Variadic function (número variável de argumentos)
func sum(numbers ...int) int {
    total := 0
    for _, n := range numbers {
        total += n
    }
    return total
}
// Uso: sum(1, 2, 3, 4, 5)
```

### Funções como Valores

```go
// Atribuir função a variável
var add = func(a, b int) int {
    return a + b
}

// Função anônima (lambda)
result := func(x int) int {
    return x * 2
}(5)  // executa imediatamente, result = 10

// Função como parâmetro
func apply(f func(int) int, value int) int {
    return f(value)
}

// Uso
double := func(x int) int { return x * 2 }
result := apply(double, 5)  // 10
```

### Defer, Panic, Recover

```go
// defer: executa no final da função (LIFO - Last In First Out)
func example() {
    defer fmt.Println("3 - Último")
    defer fmt.Println("2 - Segundo")
    fmt.Println("1 - Primeiro")
    // Output: 1, 2, 3
}

// Uso comum: fechar recursos
func readFile(filename string) error {
    file, err := os.Open(filename)
    if err != nil {
        return err
    }
    defer file.Close()  // garante que fecha mesmo se der erro

    // usar file...
    return nil
}

// panic: erro não recuperável (raramente use!)
func mustConnect(url string) {
    if url == "" {
        panic("URL não pode ser vazia")
    }
}

// recover: recupera de panic (use em defer)
func safeFunction() {
    defer func() {
        if r := recover(); r != nil {
            fmt.Println("Recuperado de panic:", r)
        }
    }()

    panic("algo deu errado")
    // código continua após recover
}
```

---

## Structs

### Definição e Uso

```go
// Definir struct
type Person struct {
    Name    string
    Age     int
    Email   string
    private string  // campo privado (começa com minúscula)
}

// Criar instância
p1 := Person{
    Name:  "João",
    Age:   30,
    Email: "joao@email.com",
}

// Ordem dos campos (não recomendado)
p2 := Person{"Maria", 25, "maria@email.com", ""}

// Struct vazio (zero values)
var p3 Person

// Acessar campos
fmt.Println(p1.Name)
p1.Age = 31

// Ponteiro para struct
p4 := &Person{Name: "Pedro"}
fmt.Println(p4.Name)  // Go faz (*p4).Name automaticamente
```

### Struct Anônima

```go
// Útil para dados temporários
user := struct {
    Name string
    Age  int
}{
    Name: "João",
    Age:  30,
}
```

### Embedded Structs (Composição)

```go
type Address struct {
    Street string
    City   string
}

type Person struct {
    Name    string
    Age     int
    Address Address  // struct dentro de struct
}

// Uso
p := Person{
    Name: "João",
    Age:  30,
    Address: Address{
        Street: "Rua A",
        City:   "São Paulo",
    },
}
fmt.Println(p.Address.City)

// Embedding anônimo (promoção de campos)
type Person struct {
    Name string
    Age  int
    Address  // sem nome de campo
}

// Acesso direto aos campos do Address
p := Person{}
p.Street = "Rua A"  // p.Address.Street
p.City = "São Paulo"
```

### Métodos

```go
type Rectangle struct {
    Width  float64
    Height float64
}

// Método com receiver de valor
func (r Rectangle) Area() float64 {
    return r.Width * r.Height
}

// Método com receiver de ponteiro (pode modificar)
func (r *Rectangle) Scale(factor float64) {
    r.Width *= factor
    r.Height *= factor
}

// Uso
rect := Rectangle{Width: 10, Height: 5}
area := rect.Area()      // 50
rect.Scale(2)            // modifica rect
newArea := rect.Area()   // 200
```

### Struct Tags

```go
type User struct {
    ID       int    `json:"id" db:"user_id" validate:"required"`
    Name     string `json:"name" db:"full_name" validate:"required,min=3"`
    Email    string `json:"email" db:"email" validate:"required,email"`
    Password string `json:"-" db:"password"`  // "-" ignora no JSON
    Age      int    `json:"age,omitempty"`    // omitempty: não inclui se for zero value
}
```

---

## Ponteiros

### Básico

```go
// Declaração
var p *int     // ponteiro para int, valor inicial nil

// & = pega endereço
x := 42
p = &x         // p aponta para x
fmt.Println(p)  // 0xc000014078 (endereço de memória)

// * = dereferencia (acessa valor)
fmt.Println(*p)  // 42
*p = 100         // modifica x através do ponteiro
fmt.Println(x)   // 100

// new: aloca memória e retorna ponteiro
p2 := new(int)   // *int com valor 0
*p2 = 42
```

### Quando Usar Ponteiros

```go
// ✅ Use ponteiro quando:

// 1. Função precisa modificar o argumento
func incrementar(n *int) {
    *n++
}
x := 5
incrementar(&x)
fmt.Println(x)  // 6

// 2. Struct grande (evita cópia)
type HugeStruct struct {
    data [1000000]int
}
func process(h *HugeStruct) {  // eficiente
    // ...
}

// 3. Precisa representar "ausência de valor"
type User struct {
    Name  string
    Age   *int  // nil = idade desconhecida
}

// ❌ Não use ponteiro quando:
// - Tipos pequenos (int, bool, float)
// - Não precisa modificar
// - Maps, slices, channels (já são referências)
```

### Ponteiros vs Valores em Métodos

```go
type Counter struct {
    count int
}

// Receiver de valor: NÃO modifica original
func (c Counter) IncrementBad() {
    c.count++  // modifica cópia
}

// Receiver de ponteiro: modifica original
func (c *Counter) IncrementGood() {
    c.count++  // modifica original
}

// Uso
c := Counter{count: 0}
c.IncrementBad()
fmt.Println(c.count)  // 0 (não mudou)

c.IncrementGood()
fmt.Println(c.count)  // 1 (mudou!)
```

---

## Arrays, Slices e Maps

### Arrays (tamanho fixo)

```go
// Declaração
var arr [5]int                    // [0 0 0 0 0]
arr2 := [5]int{1, 2, 3, 4, 5}    // [1 2 3 4 5]
arr3 := [...]int{1, 2, 3}         // tamanho inferido: [3]int

// Acesso
arr[0] = 10
value := arr[2]

// Tamanho
len(arr)  // 5

// Arrays são valores, não referências
a := [3]int{1, 2, 3}
b := a           // copia todo o array!
b[0] = 999
fmt.Println(a)   // [1 2 3]
fmt.Println(b)   // [999 2 3]
```

### Slices (tamanho dinâmico) ⭐ Use isto!

```go
// Declaração
var s []int                       // slice nil
s2 := []int{}                     // slice vazio (não nil)
s3 := []int{1, 2, 3}              // com valores
s4 := make([]int, 5)              // tamanho 5, valores 0
s5 := make([]int, 5, 10)          // tamanho 5, capacidade 10

// Append (adicionar elementos)
s := []int{1, 2, 3}
s = append(s, 4)                  // [1 2 3 4]
s = append(s, 5, 6, 7)            // [1 2 3 4 5 6 7]

// Concatenar slices
s1 := []int{1, 2}
s2 := []int{3, 4}
s3 := append(s1, s2...)           // [1 2 3 4]

// Slice de slice
s := []int{0, 1, 2, 3, 4, 5}
sub := s[1:4]                     // [1 2 3] (índices 1, 2, 3)
sub := s[:3]                      // [0 1 2] (do início até 3)
sub := s[3:]                      // [3 4 5] (de 3 até o fim)
sub := s[:]                       // cópia rasa

// Len e Cap
s := make([]int, 5, 10)
len(s)  // 5 (tamanho atual)
cap(s)  // 10 (capacidade máxima antes de realocar)

// Copiar slice
src := []int{1, 2, 3}
dst := make([]int, len(src))
copy(dst, src)                    // dst = [1 2 3]

// Remover elemento (não tem método nativo!)
s := []int{1, 2, 3, 4, 5}
i := 2  // remover índice 2
s = append(s[:i], s[i+1:]...)     // [1 2 4 5]

// Slice de structs
users := []User{
    {Name: "João", Age: 30},
    {Name: "Maria", Age: 25},
}
```

### Maps (dicionários)

```go
// Declaração
var m map[string]int              // map nil (não pode adicionar!)
m2 := map[string]int{}            // map vazio
m3 := make(map[string]int)        // map vazio
m4 := map[string]int{             // com valores
    "João":  30,
    "Maria": 25,
}

// Adicionar/Modificar
m := make(map[string]int)
m["João"] = 30
m["Maria"] = 25

// Acessar
age := m["João"]                  // 30
age := m["Pedro"]                 // 0 (não existe, retorna zero value)

// Verificar se existe
age, exists := m["João"]
if exists {
    fmt.Println("João tem", age, "anos")
} else {
    fmt.Println("João não encontrado")
}

// Deletar
delete(m, "João")

// Tamanho
len(m)  // 1

// Iterar
for key, value := range m {
    fmt.Println(key, "=", value)
}

// Só keys
for key := range m {
    fmt.Println(key)
}

// Map de structs
users := map[int]User{
    1: {Name: "João", Age: 30},
    2: {Name: "Maria", Age: 25},
}
```

---

## Controle de Fluxo

### If/Else

```go
// If básico
if x > 10 {
    fmt.Println("maior que 10")
}

// If com else
if x > 10 {
    fmt.Println("maior")
} else {
    fmt.Println("menor ou igual")
}

// If com else if
if x > 10 {
    fmt.Println("maior que 10")
} else if x > 5 {
    fmt.Println("entre 5 e 10")
} else {
    fmt.Println("5 ou menos")
}

// If com statement (variável scoped)
if err := doSomething(); err != nil {
    return err  // err só existe aqui
}
// err não existe mais aqui

// Comum com errors
if result, err := divide(10, 2); err != nil {
    fmt.Println("Erro:", err)
} else {
    fmt.Println("Resultado:", result)
}
```

### Switch

```go
// Switch básico
day := "Monday"
switch day {
case "Monday":
    fmt.Println("Segunda")
case "Tuesday":
    fmt.Println("Terça")
default:
    fmt.Println("Outro dia")
}

// Múltiplos valores no case
switch day {
case "Saturday", "Sunday":
    fmt.Println("Fim de semana!")
default:
    fmt.Println("Dia útil")
}

// Switch sem expressão (como if/else)
x := 42
switch {
case x < 0:
    fmt.Println("negativo")
case x == 0:
    fmt.Println("zero")
case x > 0:
    fmt.Println("positivo")
}

// Switch com statement
switch err := doSomething(); err {
case nil:
    fmt.Println("sucesso")
case ErrNotFound:
    fmt.Println("não encontrado")
default:
    fmt.Println("erro:", err)
}

// Type switch
var i interface{} = "hello"
switch v := i.(type) {
case int:
    fmt.Println("int:", v)
case string:
    fmt.Println("string:", v)
default:
    fmt.Println("tipo desconhecido")
}

// fallthrough: executa próximo case também
x := 1
switch x {
case 1:
    fmt.Println("um")
    fallthrough
case 2:
    fmt.Println("dois")  // executa este também
}
```

### For (único loop em Go!)

```go
// For tradicional
for i := 0; i < 10; i++ {
    fmt.Println(i)
}

// While (for sem init e post)
i := 0
for i < 10 {
    fmt.Println(i)
    i++
}

// Loop infinito
for {
    // roda para sempre
    // use break para sair
}

// Range (iterar sobre coleções)

// Slice
nums := []int{1, 2, 3, 4, 5}
for index, value := range nums {
    fmt.Println(index, value)
}

// Só valor
for _, value := range nums {
    fmt.Println(value)
}

// Só índice
for index := range nums {
    fmt.Println(index)
}

// Map
m := map[string]int{"a": 1, "b": 2}
for key, value := range m {
    fmt.Println(key, value)
}

// String (itera sobre runes)
for index, char := range "Hello" {
    fmt.Printf("%d: %c\n", index, char)
}

// Break e Continue
for i := 0; i < 10; i++ {
    if i == 5 {
        break  // sai do loop
    }
    if i%2 == 0 {
        continue  // pula para próxima iteração
    }
    fmt.Println(i)
}

// Labels (break de loops aninhados)
outer:
for i := 0; i < 3; i++ {
    for j := 0; j < 3; j++ {
        if i == 1 && j == 1 {
            break outer  // sai de ambos os loops
        }
        fmt.Println(i, j)
    }
}
```

---

## Error Handling

### Padrão de Errors

```go
// Função que retorna error
func divide(a, b float64) (float64, error) {
    if b == 0 {
        return 0, fmt.Errorf("divisão por zero")
    }
    return a / b, nil
}

// Uso típico
result, err := divide(10, 2)
if err != nil {
    // lidar com erro
    log.Fatal(err)
    return err
    // ou panic(err)
}
// usar result

// Ignorar error (não recomendado!)
result, _ := divide(10, 2)
```

### Criar Errors

```go
import "errors"

// Simples
err := errors.New("algo deu errado")

// Com formatação
err := fmt.Errorf("falha ao conectar em %s: %v", host, originalErr)

// Wrapping errors (Go 1.13+)
err := fmt.Errorf("failed to read file: %w", originalErr)

// Unwrap
errors.Unwrap(err)  // retorna originalErr

// Is: verificar tipo específico
if errors.Is(err, os.ErrNotExist) {
    fmt.Println("arquivo não existe")
}

// As: converter para tipo específico
var pathErr *os.PathError
if errors.As(err, &pathErr) {
    fmt.Println("path error:", pathErr.Path)
}
```

### Custom Errors

```go
// Error type customizado
type ValidationError struct {
    Field string
    Message string
}

func (e *ValidationError) Error() string {
    return fmt.Sprintf("%s: %s", e.Field, e.Message)
}

// Uso
func validateUser(user User) error {
    if user.Name == "" {
        return &ValidationError{
            Field: "name",
            Message: "não pode ser vazio",
        }
    }
    return nil
}

// Verificar
err := validateUser(user)
if err != nil {
    var valErr *ValidationError
    if errors.As(err, &valErr) {
        fmt.Println("Erro de validação:", valErr.Field)
    }
}
```

### Padrões Comuns

```go
// Múltiplos retornos de erro
func createUser(name string) (*User, error) {
    if name == "" {
        return nil, errors.New("name required")
    }
    user := &User{Name: name}
    return user, nil
}

// Defer para cleanup mesmo com erro
func processFile(filename string) error {
    f, err := os.Open(filename)
    if err != nil {
        return err
    }
    defer f.Close()  // sempre fecha

    // processar arquivo
    return nil
}

// Named returns para clarity
func getUserByID(id int) (user *User, err error) {
    user, err = db.Query(...)
    if err != nil {
        err = fmt.Errorf("failed to get user %d: %w", id, err)
        return
    }
    return
}
```

---

## Interfaces

### Definição e Implementação

```go
// Definir interface
type Writer interface {
    Write(data []byte) (int, error)
}

// Qualquer tipo que tenha o método Write implementa Writer
type FileWriter struct {
    filename string
}

func (f *FileWriter) Write(data []byte) (int, error) {
    // implementação
    return len(data), nil
}

// Usar interface
func saveData(w Writer, data []byte) error {
    _, err := w.Write(data)
    return err
}

// Função aceita qualquer tipo que implemente Writer
fw := &FileWriter{filename: "data.txt"}
saveData(fw, []byte("hello"))
```

### Interfaces Comuns

```go
// Stringer: usado por fmt.Println
type Stringer interface {
    String() string
}

type User struct {
    Name string
    Age  int
}

func (u User) String() string {
    return fmt.Sprintf("%s (%d years)", u.Name, u.Age)
}

// Uso
u := User{Name: "João", Age: 30}
fmt.Println(u)  // chama u.String() automaticamente

// Reader e Writer (io package)
type Reader interface {
    Read(p []byte) (n int, err error)
}

type Writer interface {
    Write(p []byte) (n int, err error)
}

// Closer
type Closer interface {
    Close() error
}

// ReadWriteCloser (composição de interfaces)
type ReadWriteCloser interface {
    Reader
    Writer
    Closer
}
```

### Interface Vazia e Type Assertions

```go
// interface{} ou any (Go 1.18+) aceita qualquer tipo
var i interface{} = "hello"
i = 42
i = true

// Type assertion
s := i.(string)  // converte, panic se não for string

// Safe type assertion
s, ok := i.(string)
if ok {
    fmt.Println("é string:", s)
} else {
    fmt.Println("não é string")
}

// Type switch
func describe(i interface{}) {
    switch v := i.(type) {
    case int:
        fmt.Println("int:", v)
    case string:
        fmt.Println("string:", v)
    case bool:
        fmt.Println("bool:", v)
    default:
        fmt.Printf("tipo desconhecido: %T\n", v)
    }
}
```

### Exemplo Prático: Repository Pattern

```go
// Interface define o contrato
type UserRepository interface {
    Create(user *User) error
    GetByID(id int) (*User, error)
    Update(user *User) error
    Delete(id int) error
}

// Implementação com PostgreSQL
type PostgresUserRepo struct {
    db *sql.DB
}

func (r *PostgresUserRepo) Create(user *User) error {
    // SQL insert
    return nil
}

func (r *PostgresUserRepo) GetByID(id int) (*User, error) {
    // SQL select
    return &User{}, nil
}

// Implementação com MongoDB
type MongoUserRepo struct {
    collection *mongo.Collection
}

func (r *MongoUserRepo) Create(user *User) error {
    // MongoDB insert
    return nil
}

// Service usa a interface, não se importa com implementação
type UserService struct {
    repo UserRepository  // interface!
}

func (s *UserService) RegisterUser(user *User) error {
    return s.repo.Create(user)  // funciona com qualquer implementação
}

// Uso
pgRepo := &PostgresUserRepo{db: db}
service := &UserService{repo: pgRepo}

// Trocar para Mongo sem mudar UserService
mongoRepo := &MongoUserRepo{collection: coll}
service = &UserService{repo: mongoRepo}
```

---

## Goroutines e Channels

### Goroutines (threads leves)

```go
// Função normal
func sayHello() {
    fmt.Println("Hello")
}

// Executar em goroutine
go sayHello()  // não bloqueia, executa concorrentemente

// Com função anônima
go func() {
    fmt.Println("Hello from goroutine")
}()

// Problema: main pode terminar antes da goroutine!
func main() {
    go sayHello()
    // main termina imediatamente, goroutine não roda
}

// Solução temporária (não faça isso em produção!)
func main() {
    go sayHello()
    time.Sleep(time.Second)  // espera goroutine terminar
}
```

### Channels (comunicação entre goroutines)

```go
// Criar channel
ch := make(chan int)         // sem buffer
ch := make(chan int, 10)     // com buffer de 10

// Enviar (bloqueia se channel estiver cheio)
ch <- 42

// Receber (bloqueia se channel estiver vazio)
value := <-ch

// Fechar channel
close(ch)

// Exemplo completo
func worker(jobs <-chan int, results chan<- int) {
    for job := range jobs {  // recebe até channel fechar
        results <- job * 2   // processa e envia resultado
    }
}

func main() {
    jobs := make(chan int, 10)
    results := make(chan int, 10)

    // Iniciar 3 workers
    for i := 0; i < 3; i++ {
        go worker(jobs, results)
    }

    // Enviar 5 jobs
    for i := 1; i <= 5; i++ {
        jobs <- i
    }
    close(jobs)  // não vai enviar mais

    // Coletar 5 resultados
    for i := 1; i <= 5; i++ {
        result := <-results
        fmt.Println(result)
    }
}
```

### Select (multiplexar channels)

```go
// Select espera em múltiplos channels
select {
case msg := <-ch1:
    fmt.Println("Recebeu de ch1:", msg)
case msg := <-ch2:
    fmt.Println("Recebeu de ch2:", msg)
case <-time.After(time.Second):
    fmt.Println("Timeout!")
}

// Default: não bloqueia
select {
case msg := <-ch:
    fmt.Println(msg)
default:
    fmt.Println("Nada para receber")
}

// Exemplo: timeout
func getDataWithTimeout(url string) (string, error) {
    dataCh := make(chan string)

    go func() {
        data := fetchData(url)  // função lenta
        dataCh <- data
    }()

    select {
    case data := <-dataCh:
        return data, nil
    case <-time.After(5 * time.Second):
        return "", errors.New("timeout")
    }
}
```

### WaitGroup (esperar goroutines terminarem)

```go
import "sync"

func main() {
    var wg sync.WaitGroup

    for i := 0; i < 5; i++ {
        wg.Add(1)  // incrementa contador

        go func(id int) {
            defer wg.Done()  // decrementa quando terminar

            fmt.Println("Worker", id, "started")
            time.Sleep(time.Second)
            fmt.Println("Worker", id, "done")
        }(i)
    }

    wg.Wait()  // bloqueia até todos terminarem
    fmt.Println("Todos terminaram!")
}
```

### Mutex (proteger dados compartilhados)

```go
import "sync"

type SafeCounter struct {
    mu    sync.Mutex
    count int
}

func (c *SafeCounter) Increment() {
    c.mu.Lock()
    defer c.mu.Unlock()
    c.count++
}

func (c *SafeCounter) Value() int {
    c.mu.Lock()
    defer c.mu.Unlock()
    return c.count
}

// Uso
counter := &SafeCounter{}
var wg sync.WaitGroup

for i := 0; i < 1000; i++ {
    wg.Add(1)
    go func() {
        defer wg.Done()
        counter.Increment()
    }()
}

wg.Wait()
fmt.Println(counter.Value())  // 1000 (sem race condition!)
```

### Context (cancelamento e timeout)

```go
import "context"

// Context com timeout
ctx, cancel := context.WithTimeout(context.Background(), 5*time.Second)
defer cancel()

// Context com cancelamento manual
ctx, cancel := context.WithCancel(context.Background())
defer cancel()

// Usar em função
func doWork(ctx context.Context) error {
    for {
        select {
        case <-ctx.Done():
            return ctx.Err()  // context canceled ou timeout
        default:
            // fazer trabalho
            time.Sleep(100 * time.Millisecond)
        }
    }
}

// Exemplo HTTP com context
req, err := http.NewRequestWithContext(ctx, "GET", url, nil)
resp, err := client.Do(req)
```

---

## Packages e Imports

### Estrutura de Package

```go
// arquivo: myapp/math/add.go
package math  // nome do package

func Add(a, b int) int {  // maiúscula = exportado (público)
    return helper(a) + helper(b)
}

func helper(x int) int {  // minúscula = privado
    return x
}

// arquivo: myapp/main.go
package main

import "myapp/math"  // importa package

func main() {
    result := math.Add(1, 2)  // usa função exportada
    // math.helper() // ERRO: helper não é exportado
}
```

### Import Patterns

```go
// Import simples
import "fmt"
import "os"

// Import múltiplo (preferido)
import (
    "fmt"
    "os"
    "strings"
)

// Import com alias
import (
    f "fmt"          // usa f.Println()
    "os"
)

// Import local package
import (
    "github.com/meu/projeto/internal/models"
    "github.com/meu/projeto/pkg/logger"
)

// Dot import (não recomendado!)
import . "fmt"
// Println() sem prefixo

// Import só para side effects
import _ "github.com/lib/pq"  // registra driver SQL
```

### go.mod e Modules

```bash
# Criar módulo
go mod init github.com/usuario/projeto

# Adicionar dependência
go get github.com/gin-gonic/gin
go get github.com/gin-gonic/gin@v1.9.0  # versão específica

# Atualizar
go get -u github.com/gin-gonic/gin

# Remover não usadas
go mod tidy

# Verificar dependências
go list -m all
go mod why github.com/gin-gonic/gin
```

### init() Function

```go
// Executada automaticamente antes de main()
func init() {
    fmt.Println("Inicializando package")
    // setup, configuração
}

// Pode ter múltiplos init() no mesmo package
func init() {
    fmt.Println("Outro init")
}

// Ordem: imports → constantes → variáveis → init() → main()
```

---

## JSON

### Marshal (Go → JSON)

```go
import "encoding/json"

type User struct {
    ID       int    `json:"id"`
    Name     string `json:"name"`
    Email    string `json:"email"`
    Password string `json:"-"`              // não serializa
    Age      int    `json:"age,omitempty"`  // omite se zero
}

user := User{
    ID:       1,
    Name:     "João",
    Email:    "joao@email.com",
    Password: "secret",
}

// Marshal para []byte
jsonBytes, err := json.Marshal(user)
if err != nil {
    log.Fatal(err)
}
fmt.Println(string(jsonBytes))
// {"id":1,"name":"João","email":"joao@email.com"}

// MarshalIndent (formatado)
jsonBytes, err := json.MarshalIndent(user, "", "  ")
```

### Unmarshal (JSON → Go)

```go
jsonStr := `{"id":1,"name":"João","email":"joao@email.com"}`

var user User
err := json.Unmarshal([]byte(jsonStr), &user)
if err != nil {
    log.Fatal(err)
}
fmt.Println(user.Name)  // João

// Unmarshal de array
jsonArray := `[
    {"id":1,"name":"João"},
    {"id":2,"name":"Maria"}
]`

var users []User
err := json.Unmarshal([]byte(jsonArray), &users)
```

### JSON com Maps

```go
// JSON para map[string]interface{}
jsonStr := `{"name":"João","age":30,"active":true}`

var data map[string]interface{}
json.Unmarshal([]byte(jsonStr), &data)

name := data["name"].(string)  // type assertion
age := data["age"].(float64)   // JSON numbers são float64!
```

### Encoder e Decoder (streams)

```go
// Encoder (escrever para Writer)
file, _ := os.Create("data.json")
defer file.Close()

encoder := json.NewEncoder(file)
encoder.SetIndent("", "  ")
err := encoder.Encode(user)

// Decoder (ler de Reader)
file, _ := os.Open("data.json")
defer file.Close()

decoder := json.NewDecoder(file)
var user User
err := decoder.Decode(&user)

// HTTP Response
resp, _ := http.Get("https://api.example.com/user")
defer resp.Body.Close()

var user User
json.NewDecoder(resp.Body).Decode(&user)
```

### Custom JSON Marshaling

```go
import "time"

type Event struct {
    Name string
    Date time.Time
}

// Custom marshal
func (e Event) MarshalJSON() ([]byte, error) {
    type Alias Event  // evita recursão
    return json.Marshal(&struct {
        Date string `json:"date"`
        *Alias
    }{
        Date:  e.Date.Format("2006-01-02"),
        Alias: (*Alias)(&e),
    })
}

// Custom unmarshal
func (e *Event) UnmarshalJSON(data []byte) error {
    type Alias Event
    aux := &struct {
        Date string `json:"date"`
        *Alias
    }{
        Alias: (*Alias)(e),
    }

    if err := json.Unmarshal(data, &aux); err != nil {
        return err
    }

    date, err := time.Parse("2006-01-02", aux.Date)
    if err != nil {
        return err
    }
    e.Date = date
    return nil
}
```

---

## Testes

### Teste Básico

```go
// arquivo: math.go
package math

func Add(a, b int) int {
    return a + b
}

// arquivo: math_test.go
package math

import "testing"

func TestAdd(t *testing.T) {
    result := Add(2, 3)
    expected := 5

    if result != expected {
        t.Errorf("Add(2, 3) = %d; esperado %d", result, expected)
    }
}

// Rodar: go test
```

### Table-Driven Tests

```go
func TestAdd(t *testing.T) {
    tests := []struct {
        name     string
        a, b     int
        expected int
    }{
        {"positivos", 2, 3, 5},
        {"negativos", -2, -3, -5},
        {"zero", 0, 5, 5},
        {"misturado", -2, 3, 1},
    }

    for _, tt := range tests {
        t.Run(tt.name, func(t *testing.T) {
            result := Add(tt.a, tt.b)
            if result != tt.expected {
                t.Errorf("Add(%d, %d) = %d; esperado %d",
                    tt.a, tt.b, result, tt.expected)
            }
        })
    }
}

// Rodar: go test -v
```

### Test Helpers

```go
func assertEqual(t *testing.T, got, want int) {
    t.Helper()  // marca como helper (não aparece em stack trace)
    if got != want {
        t.Errorf("got %d, want %d", got, want)
    }
}

func TestSomething(t *testing.T) {
    result := doSomething()
    assertEqual(t, result, 42)
}
```

### Subtestes

```go
func TestUser(t *testing.T) {
    t.Run("Create", func(t *testing.T) {
        // teste de criação
    })

    t.Run("Update", func(t *testing.T) {
        // teste de atualização
    })

    t.Run("Delete", func(t *testing.T) {
        // teste de deleção
    })
}

// Rodar teste específico: go test -run TestUser/Create
```

### Setup e Teardown

```go
func TestMain(m *testing.M) {
    // Setup antes de todos os testes
    fmt.Println("Setup")

    // Rodar testes
    code := m.Run()

    // Teardown depois de todos os testes
    fmt.Println("Teardown")

    os.Exit(code)
}

func TestSomething(t *testing.T) {
    // Setup para este teste
    setup()
    defer teardown()

    // teste aqui
}
```

### Benchmarks

```go
func BenchmarkAdd(b *testing.B) {
    for i := 0; i < b.N; i++ {
        Add(2, 3)
    }
}

// Rodar: go test -bench=.
// Output: BenchmarkAdd-8   500000000   2.50 ns/op
```

### Mocking

```go
// Interface para injeção de dependência
type UserRepository interface {
    GetByID(id int) (*User, error)
}

// Mock
type MockUserRepository struct {
    users map[int]*User
}

func (m *MockUserRepository) GetByID(id int) (*User, error) {
    user, ok := m.users[id]
    if !ok {
        return nil, errors.New("not found")
    }
    return user, nil
}

// Teste
func TestUserService(t *testing.T) {
    mockRepo := &MockUserRepository{
        users: map[int]*User{
            1: {Name: "João"},
        },
    }

    service := NewUserService(mockRepo)
    user, err := service.GetUser(1)

    if err != nil {
        t.Fatal(err)
    }
    if user.Name != "João" {
        t.Errorf("esperado João, got %s", user.Name)
    }
}
```

---

## Dicas e Truques

### Strings

```go
import "strings"

// Concatenar (ineficiente)
s := "Hello" + " " + "World"

// Builder (eficiente para múltiplas concatenações)
var b strings.Builder
b.WriteString("Hello")
b.WriteString(" ")
b.WriteString("World")
s := b.String()

// Operações comuns
strings.Contains("hello", "ll")      // true
strings.HasPrefix("hello", "he")    // true
strings.HasSuffix("hello", "lo")    // true
strings.ToLower("HELLO")            // "hello"
strings.ToUpper("hello")            // "HELLO"
strings.TrimSpace("  hello  ")      // "hello"
strings.Split("a,b,c", ",")         // ["a", "b", "c"]
strings.Join([]string{"a", "b"}, ",")  // "a,b"
strings.Repeat("ha", 3)             // "hahaha"
strings.Replace("hello", "l", "L", 1)  // "heLlo"
strings.ReplaceAll("hello", "l", "L")  // "heLLo"
```

### Time

```go
import "time"

// Tempo atual
now := time.Now()

// Criar tempo específico
t := time.Date(2025, time.January, 1, 0, 0, 0, 0, time.UTC)

// Parsing
layout := "2006-01-02 15:04:05"  // formato de referência do Go!
t, err := time.Parse(layout, "2025-01-01 10:30:00")

// Formatação
s := t.Format("2006-01-02")           // "2025-01-01"
s := t.Format("02/01/2006")           // "01/01/2025"
s := t.Format("15:04:05")             // "10:30:00"
s := t.Format(time.RFC3339)           // formato padrão

// Operações
t2 := t.Add(24 * time.Hour)           // adiciona 1 dia
t2 := t.AddDate(0, 1, 0)              // adiciona 1 mês
duration := t2.Sub(t)                 // duração entre tempos
t.Before(t2)                          // true
t.After(t2)                           // false

// Sleep
time.Sleep(time.Second)
time.Sleep(100 * time.Millisecond)

// Timer
timer := time.NewTimer(5 * time.Second)
<-timer.C  // bloqueia por 5 segundos

// Ticker
ticker := time.NewTicker(time.Second)
defer ticker.Stop()
for t := range ticker.C {
    fmt.Println("tick", t)
}
```

### Random

```go
import (
    "math/rand"
    "time"
)

// Seed (use uma vez no início do programa)
rand.Seed(time.Now().UnixNano())

// Números aleatórios
n := rand.Int()              // int aleatório
n := rand.Intn(100)          // 0 a 99
f := rand.Float64()          // 0.0 a 1.0

// Slice aleatório
rand.Shuffle(len(slice), func(i, j int) {
    slice[i], slice[j] = slice[j], slice[i]
})
```

### Files

```go
import (
    "io"
    "os"
)

// Ler arquivo completo
data, err := os.ReadFile("file.txt")

// Escrever arquivo
err := os.WriteFile("file.txt", []byte("hello"), 0644)

// Abrir arquivo
file, err := os.Open("file.txt")
defer file.Close()

// Criar arquivo
file, err := os.Create("file.txt")
defer file.Close()

// Ler linha por linha
scanner := bufio.NewScanner(file)
for scanner.Scan() {
    line := scanner.Text()
    fmt.Println(line)
}

// Copiar arquivo
src, _ := os.Open("src.txt")
defer src.Close()
dst, _ := os.Create("dst.txt")
defer dst.Close()
io.Copy(dst, src)

// Verificar se existe
if _, err := os.Stat("file.txt"); os.IsNotExist(err) {
    fmt.Println("arquivo não existe")
}

// Remover
os.Remove("file.txt")

// Criar diretório
os.Mkdir("dir", 0755)
os.MkdirAll("path/to/dir", 0755)  // cria path completo
```

### HTTP Client

```go
import "net/http"

// GET simples
resp, err := http.Get("https://api.example.com/data")
if err != nil {
    log.Fatal(err)
}
defer resp.Body.Close()

body, err := io.ReadAll(resp.Body)

// POST JSON
data := map[string]string{"name": "João"}
jsonData, _ := json.Marshal(data)

resp, err := http.Post(
    "https://api.example.com/users",
    "application/json",
    bytes.NewBuffer(jsonData),
)

// Request customizado
req, err := http.NewRequest("POST", url, bytes.NewBuffer(jsonData))
req.Header.Set("Content-Type", "application/json")
req.Header.Set("Authorization", "Bearer token")

client := &http.Client{Timeout: 10 * time.Second}
resp, err := client.Do(req)
```

### Úteis

```go
// Defer múltiplos (executam em ordem reversa)
func example() {
    defer fmt.Println("1")
    defer fmt.Println("2")
    defer fmt.Println("3")
    // Output: 3, 2, 1
}

// Blank identifier (ignorar valores)
_, err := doSomething()  // ignora primeiro retorno
for _, value := range slice {}  // ignora índice

// Múltiplas atribuições
a, b = b, a  // swap sem variável temporária

// Type alias
type UserID int
type Email string

// Conversão entre tipos compatíveis
var id UserID = 42
var num int = int(id)

// Verificar tipo em tempo de execução
var i interface{} = "hello"
fmt.Printf("Type: %T\n", i)  // Type: string
```

---

## Erros Comuns e Soluções

### 1. Esqueceu de usar variável

```go
// ❌ ERRO: declared but not used
func main() {
    x := 42
}

// ✅ SOLUÇÃO: use blank identifier se realmente não vai usar
func main() {
    _ = 42  // ou remova a linha
}
```

### 2. Return values

```go
// ❌ ERRO: not enough return values
func divide(a, b int) (int, error) {
    return a / b  // falta error
}

// ✅ SOLUÇÃO
func divide(a, b int) (int, error) {
    if b == 0 {
        return 0, errors.New("divisão por zero")
    }
    return a / b, nil
}
```

### 3. Slice gotcha

```go
// ❌ PROBLEMA: slice é referência
s1 := []int{1, 2, 3}
s2 := s1  // s2 aponta para mesmo array!
s2[0] = 999
fmt.Println(s1)  // [999 2 3]

// ✅ SOLUÇÃO: copiar
s2 := make([]int, len(s1))
copy(s2, s1)
```

### 4. Goroutine loop variable

```go
// ❌ PROBLEMA
for i := 0; i < 5; i++ {
    go func() {
        fmt.Println(i)  // imprime 5, 5, 5, 5, 5
    }()
}

// ✅ SOLUÇÃO: passar como parâmetro
for i := 0; i < 5; i++ {
    go func(n int) {
        fmt.Println(n)  // imprime 0, 1, 2, 3, 4
    }(i)
}
```

### 5. Nil pointer dereference

```go
// ❌ ERRO: panic
var p *int
*p = 42  // panic!

// ✅ SOLUÇÃO: inicializar
p := new(int)
*p = 42

// Ou
x := 42
p := &x
```

---

## Boas Práticas e Convenções

### Nomenclatura

```go
// ✅ BOM

// Pacotes: minúsculo, uma palavra, sem underscore
package user
package httputil

// Variáveis/funções privadas: camelCase começando com minúscula
func getUserByID(id int) (*User, error)
var maxRetries = 3

// Variáveis/funções públicas: PascalCase começando com maiúscula
func CreateUser(name string) error
var DefaultTimeout = 30 * time.Second

// Constantes: PascalCase ou SCREAMING_SNAKE_CASE para grupos
const MaxConnections = 100
const (
    StatusActive   = "active"
    StatusInactive = "inactive"
)

// Interfaces: nome do método + "er" (quando faz sentido)
type Reader interface {
    Read(p []byte) (n int, err error)
}
type Stringer interface {
    String() string
}

// ❌ EVITE

// Underscore em nomes (não é idiomático)
func get_user_by_id(id int) (*User, error)
var max_retries = 3

// Abreviações inconsistentes
func GetUsr(id int) (*User, error)  // use GetUser
var usrID int  // use userID

// Nomes genéricos demais
var data []byte      // data de quê? use responseData, configData
func handle() error  // handle o quê? use handleRequest

// Stutter (repetição)
type UserUser struct {}  // só User
package user
func UserCreate() // dentro do package user, só Create()
```

### Organização de Código

```go
// ✅ ORDEM RECOMENDADA dentro de um arquivo

package main

// 1. Imports (agrupados e ordenados)
import (
    // stdlib primeiro
    "context"
    "fmt"
    "time"

    // libs externas depois
    "github.com/gin-gonic/gin"

    // seu código por último
    "github.com/seu/projeto/internal/models"
)

// 2. Constantes
const (
    DefaultPort = 8080
    MaxRetries  = 3
)

// 3. Variáveis globais (evite quando possível!)
var (
    ErrNotFound = errors.New("not found")
    logger      *zap.Logger
)

// 4. Tipos
type Config struct {
    Port int
}

type UserService struct {
    repo UserRepository
}

// 5. Funções construtoras
func NewUserService(repo UserRepository) *UserService {
    return &UserService{repo: repo}
}

// 6. Métodos (agrupados por receiver)
func (s *UserService) Create(user *User) error {
    // ...
}

func (s *UserService) GetByID(id int) (*User, error) {
    // ...
}

// 7. Funções auxiliares privadas
func validateEmail(email string) bool {
    // ...
}
```

### Error Handling

```go
// ✅ BOM

// Sempre checar errors
result, err := doSomething()
if err != nil {
    return fmt.Errorf("failed to do something: %w", err)  // wrap error
}

// Múltiplos returns: error sempre por último
func GetUser(id int) (*User, error) { }
func Divide(a, b int) (int, error) { }

// Nomear errors exportados com prefixo Err
var (
    ErrNotFound      = errors.New("not found")
    ErrInvalidInput  = errors.New("invalid input")
    ErrUnauthorized  = errors.New("unauthorized")
)

// Custom errors para mais contexto
type ValidationError struct {
    Field   string
    Message string
}

func (e *ValidationError) Error() string {
    return fmt.Sprintf("%s: %s", e.Field, e.Message)
}

// ❌ EVITE

// Ignorar error sem motivo
result, _ := doSomething()  // só faça se REALMENTE pode ignorar

// Panic em código de biblioteca
func GetUser(id int) *User {
    user, err := db.Query(...)
    if err != nil {
        panic(err)  // ❌ deixe quem chama decidir
    }
    return user
}

// Error sem contexto
if err != nil {
    return err  // ❌ erro genérico
}
// ✅ Melhor:
if err != nil {
    return fmt.Errorf("failed to get user %d: %w", id, err)
}
```

### Inicialização e Construtores

```go
// ✅ BOM

// Use funções New* para construtores
func NewServer(port int) *Server {
    return &Server{
        port:    port,
        timeout: 30 * time.Second,  // valores padrão sensatos
    }
}

// Use Options pattern para muitas configurações
type ServerOption func(*Server)

func WithTimeout(d time.Duration) ServerOption {
    return func(s *Server) {
        s.timeout = d
    }
}

func NewServer(port int, opts ...ServerOption) *Server {
    s := &Server{
        port:    port,
        timeout: 30 * time.Second,  // padrão
    }
    for _, opt := range opts {
        opt(s)
    }
    return s
}

// Uso: server := NewServer(8080, WithTimeout(time.Minute))

// ❌ EVITE

// Structs com muitos parâmetros obrigatórios
func NewServer(port int, timeout time.Duration, maxConn int,
    readTimeout time.Duration, writeTimeout time.Duration) *Server {
    // muito verboso
}

// Exportar structs com campos não inicializados críticos
type Server struct {
    DB *sql.DB  // se nil, vai dar panic!
}
// ✅ Force inicialização via construtor
```

### Interfaces

```go
// ✅ BOM

// Interfaces pequenas (1-3 métodos)
type Reader interface {
    Read(p []byte) (n int, err error)
}

type Writer interface {
    Write(p []byte) (n int, err error)
}

// Declare interface onde é usada, não onde é implementada
// ❌ package database
type UserRepository interface {
    GetByID(int) (*User, error)
}

// ✅ package userservice
type UserRepository interface {  // só o que service precisa
    GetByID(int) (*User, error)
}

// Accept interfaces, return structs
func ProcessData(r io.Reader) (*Result, error) {  // ✅ aceita interface
    // ...
    return &Result{}, nil  // ✅ retorna concrete type
}

// ❌ EVITE

// Interface muito grande (God interface)
type Service interface {
    Create(...) error
    Update(...) error
    Delete(...) error
    GetByID(...) (*User, error)
    List(...) ([]*User, error)
    Search(...) ([]*User, error)
    // ... 20 métodos
}
// ✅ Quebre em interfaces menores

// Poluir com interfaces desnecessárias
type UserStruct struct { Name string }
type UserInterface interface { GetName() string }
// Só crie interface quando REALMENTE precisar de abstração
```

### Concorrência

```go
// ✅ BOM

// Use sync.WaitGroup para esperar goroutines
var wg sync.WaitGroup
for i := 0; i < 10; i++ {
    wg.Add(1)
    go func(n int) {  // ✅ passa variável como parâmetro
        defer wg.Done()
        process(n)
    }(i)
}
wg.Wait()

// Use context para cancelamento
func doWork(ctx context.Context) error {
    for {
        select {
        case <-ctx.Done():
            return ctx.Err()
        default:
            // trabalho
        }
    }
}

// Proteja dados compartilhados com mutex
type SafeCounter struct {
    mu    sync.Mutex
    count int
}

func (c *SafeCounter) Inc() {
    c.mu.Lock()
    defer c.mu.Unlock()
    c.count++
}

// Channels com buffer para produtor/consumidor
jobs := make(chan Job, 100)  // ✅ buffer evita deadlock

// ❌ EVITE

// Goroutine leak (não tem como parar)
go func() {
    for {
        doWork()  // ❌ roda para sempre, sem cancelamento
    }
}()

// Capturar variável de loop
for i := 0; i < 5; i++ {
    go func() {
        fmt.Println(i)  // ❌ todas goroutines veem mesmo i
    }()
}

// Acessar dados sem proteção
type Counter struct {
    count int  // ❌ race condition!
}
func (c *Counter) Inc() { c.count++ }

// Channel sem buffer bloqueante
ch := make(chan int)  // cuidado: pode causar deadlock
ch <- 42  // ❌ bloqueia se ninguém estiver recebendo
```

### Ponteiros e Valores

```go
// ✅ QUANDO USAR PONTEIROS

// 1. Método precisa modificar receiver
func (u *User) SetName(name string) {
    u.Name = name  // modifica original
}

// 2. Struct grande (evita cópia)
type LargeStruct struct {
    data [10000]int
}
func process(l *LargeStruct) { }  // ✅ eficiente

// 3. Precisa representar "ausência" (nil)
type Config struct {
    Timeout *int  // nil = usar padrão
}

// 4. Consistência: se um método usa *T, use em todos
func (u *User) Save() error { }
func (u *User) Delete() error { }  // ✅ consistente

// ✅ QUANDO USAR VALORES

// 1. Tipos pequenos (< 100 bytes)
func formatDate(d time.Time) string { }

// 2. Não precisa modificar
func (u User) GetName() string {
    return u.Name  // só lê
}

// 3. Maps, slices, channels (já são referências)
func process(m map[string]int) { }  // não precisa de *map
```

### Comentários e Documentação

```go
// ✅ BOM

// Package comment: explica propósito do package (no arquivo principal)
// Package user provides user management functionality.
//
// It handles user creation, authentication, and profile management.
package user

// Funções/tipos públicos: comentário começa com o nome
// GetByID retrieves a user by their ID.
// Returns ErrNotFound if user doesn't exist.
func GetByID(id int) (*User, error) { }

// User represents a system user.
type User struct {
    ID   int
    Name string
}

// Constantes: agrupe comentários relacionados
const (
    // User statuses
    StatusActive   = "active"
    StatusInactive = "inactive"
    StatusBanned   = "banned"
)

// TODO comments para trabalho futuro
// TODO(username): implementar cache aqui
// FIXME: bug quando user é nil

// ❌ EVITE

// Comentários óbvios
// Get ID
func (u User) GetID() int { return u.ID }

// Comentários que contradizem código
// Returns true if user is active
func (u User) IsActive() string {  // ❌ retorna string, não bool!
    return u.Status
}

// Comentários desatualizados
// Connects to MySQL database
func connectDB() {
    // ... conecta PostgreSQL ❌
}
```

### Organização de Pacotes

```go
// ✅ BOA ESTRUTURA

myapp/
├── cmd/
│   ├── api/main.go        // executável da API
│   └── worker/main.go     // executável do worker
├── internal/              // código privado (não importável)
│   ├── user/
│   │   ├── user.go       // tipos
│   │   ├── repository.go // interface
│   │   └── service.go    // lógica
│   └── auth/
├── pkg/                   // código público (reutilizável)
│   ├── logger/
│   └── errors/
└── go.mod

// ✅ Package por domínio/feature, não por tipo
internal/
├── user/          // ✅ tudo relacionado a user
│   ├── user.go
│   ├── repository.go
│   └── service.go
└── product/

// ❌ Package por tipo (estilo MVC)
internal/
├── models/        // ❌ todos os models juntos
├── repositories/  // ❌ todos os repos juntos
└── services/      // ❌ todos os services juntos

// ✅ NOMES DE PACOTES

// Bom: curto, descritivo, singular
package user
package http
package auth

// Evite
package users        // ❌ prefira singular
package user_service // ❌ sem underscore
package userservice  // ✅ ou junte as palavras
package util         // ❌ muito genérico
package common       // ❌ muito genérico
```

### Performance

```go
// ✅ OTIMIZAÇÕES COMUNS

// 1. Prealoque slices se souber o tamanho
users := make([]User, 0, 100)  // ✅ capacidade 100

// 2. Use strings.Builder para concatenação
var b strings.Builder
for _, s := range words {
    b.WriteString(s)
}
result := b.String()

// 3. Evite alocações em loops
// ❌ Ruim
for i := 0; i < n; i++ {
    temp := fmt.Sprintf("%d", i)  // aloca a cada iteração
}
// ✅ Melhor
var buf bytes.Buffer
for i := 0; i < n; i++ {
    buf.Reset()
    fmt.Fprintf(&buf, "%d", i)
}

// 4. Use sync.Pool para objetos reutilizáveis
var bufferPool = sync.Pool{
    New: func() interface{} {
        return new(bytes.Buffer)
    },
}

buf := bufferPool.Get().(*bytes.Buffer)
defer bufferPool.Put(buf)
buf.Reset()
// use buf

// 5. Channel buffering apropriado
results := make(chan Result, workerCount)  // ✅ evita blocking
```

### Testes

```go
// ✅ BOM

// Nome de teste: Test + nome da função/feature
func TestUserCreate(t *testing.T) { }
func TestUserValidation(t *testing.T) { }

// Use subtestes para casos relacionados
func TestUser(t *testing.T) {
    t.Run("Create", func(t *testing.T) { })
    t.Run("Update", func(t *testing.T) { })
}

// Table-driven tests
func TestAdd(t *testing.T) {
    tests := []struct {
        name string
        a, b int
        want int
    }{
        {"positive", 1, 2, 3},
        {"negative", -1, -1, -2},
        {"zero", 0, 5, 5},
    }
    for _, tt := range tests {
        t.Run(tt.name, func(t *testing.T) {
            got := Add(tt.a, tt.b)
            if got != tt.want {
                t.Errorf("got %d, want %d", got, tt.want)
            }
        })
    }
}

// Helpers com t.Helper()
func assertNoError(t *testing.T, err error) {
    t.Helper()  // mostra linha que chamou, não esta linha
    if err != nil {
        t.Fatal(err)
    }
}

// Teste de behavior, não implementação
// ✅ testa o QUE faz
func TestUserService_Create_ValidInput_Success(t *testing.T) { }
// ❌ testa COMO faz
func TestUserService_CallsRepositoryInsert(t *testing.T) { }
```

### Código Limpo

```go
// ✅ BOM

// Early return para reduzir aninhamento
func doSomething(x int) error {
    if x < 0 {
        return errors.New("negative")
    }
    if x == 0 {
        return errors.New("zero")
    }
    // lógica principal aqui, sem muito indentação
    return nil
}

// Funções pequenas (< 50 linhas idealmente)
func processUser(u *User) error {
    if err := validate(u); err != nil {
        return err
    }
    if err := save(u); err != nil {
        return err
    }
    return notify(u)
}

// Nome de variáveis: curto em escopo pequeno, descritivo em escopo grande
func sum(nums []int) int {
    s := 0  // ✅ curto, escopo pequeno
    for _, n := range nums {
        s += n
    }
    return s
}

var globalDatabaseConnection *sql.DB  // ✅ descritivo, escopo grande

// ❌ EVITE

// Aninhamento profundo (> 3 níveis)
func doSomething() {
    if cond1 {
        if cond2 {
            if cond3 {
                if cond4 {
                    // ❌ difícil de ler
                }
            }
        }
    }
}

// Funções gigantes (> 100 linhas)
func handleEverything() {
    // ❌ quebre em funções menores
}

// Variáveis com nome ruim
func processData() {
    d := getData()     // ❌ o que é d?
    x := transform(d)  // ❌ o que é x?
    // ✅ use: rawData, transformedData
}
```

### Segurança

```go
// ✅ BOM

// Não logue informações sensíveis
log.Info("user logged in", zap.Int("user_id", user.ID))  // ✅
// ❌ NÃO: zap.String("password", user.Password)

// Use crypto/rand para valores aleatórios seguros
import "crypto/rand"
token := make([]byte, 32)
rand.Read(token)  // ✅ criptograficamente seguro

// Valide e sanitize inputs
func GetUser(id string) (*User, error) {
    userID, err := strconv.Atoi(id)
    if err != nil {
        return nil, errors.New("invalid user ID")
    }
    if userID < 0 {
        return nil, errors.New("user ID must be positive")
    }
    // ...
}

// Use prepared statements para SQL
db.Query("SELECT * FROM users WHERE id = ?", userID)  // ✅ safe
// ❌ NÃO: db.Query("SELECT * FROM users WHERE id = " + userID)

// ❌ EVITE

// math/rand para segurança
import "math/rand"
token := rand.Intn(1000)  // ❌ previsível!

// Hardcoded secrets
const APIKey = "secret123"  // ❌ use env vars

// SQL injection vulnerável
query := "SELECT * FROM users WHERE name = '" + name + "'"  // ❌
```

---

## Ferramentas Essenciais

### Linting e Formatação

```bash
# Formatar código (SEMPRE use!)
go fmt ./...
gofmt -w .

# Linting (encontra bugs)
go vet ./...

# golangci-lint (meta-linter completo)
# Instalar: go install github.com/golangci/golangci-lint/cmd/golangci-lint@latest
golangci-lint run

# staticcheck (análise estática avançada)
# Instalar: go install honnef.co/go/tools/cmd/staticcheck@latest
staticcheck ./...
```

### Checklist Antes de Commitar

```bash
# 1. Formatar
go fmt ./...

# 2. Vet
go vet ./...

# 3. Testes
go test ./...

# 4. Coverage
go test -cover ./...

# 5. Race detector
go test -race ./...

# 6. Build
go build ./...
```

---

## Links Úteis

- [Go Playground](https://play.golang.org) - Teste código online
- [Go by Example](https://gobyexample.com) - Exemplos práticos
- [Effective Go](https://go.dev/doc/effective_go) - Best practices oficiais
- [Go Code Review Comments](https://github.com/golang/go/wiki/CodeReviewComments) - Estilo de código
- [Uber Go Style Guide](https://github.com/uber-go/guide/blob/master/style.md) - Guia completo
- [Go Doc](https://pkg.go.dev) - Documentação de pacotes
- [Go Wiki](https://github.com/golang/go/wiki) - Recursos da comunidade

---

## Checklist de Qualidade

Use esta checklist ao revisar seu código:

- [ ] Código formatado com `go fmt`?
- [ ] Sem warnings do `go vet`?
- [ ] Todos os exports têm comentários?
- [ ] Errors são tratados (não ignorados)?
- [ ] Errors wrappados com contexto?
- [ ] Recursos fechados com `defer`?
- [ ] Testes escritos para novas funcionalidades?
- [ ] Sem segredos (senhas, keys) no código?
- [ ] Nomes descritivos e idiomáticos?
- [ ] Funções < 50 linhas?
- [ ] Aninhamento < 3 níveis?
- [ ] Race detector passou (`go test -race`)?

---

**Dica Final**: Mantenha este arquivo aberto enquanto codifica. Use Ctrl+F para buscar conceitos rapidamente!
