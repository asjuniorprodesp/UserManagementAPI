# UserManagementAPI - Documentacao Simples

API de gerenciamento de usuarios da TechHive Solutions.

## Base URL

Use a URL conforme o perfil da aplicacao:

- HTTPS: `https://localhost:7133`
- HTTP: `http://localhost:5027`

## Entidade User

Campos retornados pela API:

- `userId` (int)
- `fullName` (string)
- `email` (string)
- `department` (string)
- `role` (string)
- `isActive` (bool)
- `createdAt` (datetime)
- `updatedAt` (datetime|null)

## Endpoints

### 1. Listar usuarios

**GET** `/users`

Resposta de sucesso:

- **200 OK**

Observacao de desempenho:

- Esse endpoint usa cache em memoria por 30 segundos para reduzir carga no banco.
- O cache e invalidado automaticamente em criacao, atualizacao e exclusao de usuario.

Exemplo:

```json
[
  {
    "userId": 1,
    "fullName": "Ana Beatriz Ramos",
    "email": "ana.ramos@techhive.com",
    "department": "RH",
    "role": "Analista de Recursos Humanos",
    "isActive": true,
    "createdAt": "2026-03-18T12:00:00Z",
    "updatedAt": null
  }
]
```

### 2. Buscar usuario por ID

**GET** `/users/{id}`

Resposta:

- **200 OK** quando encontrado
- **404 Not Found** quando nao encontrado
- **500 Internal Server Error** em caso de falha no banco

Mensagem de erro (404):

```json
"Usuario com Id 99 nao encontrado."
```

### 3. Criar usuario

**POST** `/users`

Body:

```json
{
  "fullName": "Joao Paulo Souza",
  "email": "joao.souza@techhive.com",
  "department": "TI",
  "role": "Desenvolvedor Full Stack",
  "isActive": true
}
```

Resposta:

- **201 Created** com o usuario criado
- **400 Bad Request** com ValidationProblem para payload invalido
- **500 Internal Server Error** em caso de falha no banco

Campos obrigatorios:

- `fullName`
- `email`
- `department`
- `role`

Regras de validacao:

- `fullName`: obrigatorio, 2 a 150 caracteres, nao pode ser somente espacos.
- `email`: obrigatorio, formato de e-mail valido, maximo de 150 caracteres.
- `department`: obrigatorio, 2 a 100 caracteres, nao pode ser somente espacos.
- `role`: obrigatorio, 2 a 100 caracteres, nao pode ser somente espacos.

### 4. Atualizar usuario

**PUT** `/users/{id}`

Body:

```json
{
  "fullName": "Ana Beatriz Ramos",
  "email": "ana.ramos@techhive.com",
  "department": "RH",
  "role": "Gerente de Recursos Humanos",
  "isActive": true
}
```

Resposta:

- **200 OK** quando atualizado
- **400 Bad Request** com ValidationProblem para payload invalido
- **404 Not Found** quando o usuario nao existir
- **500 Internal Server Error** em caso de falha no banco

### 5. Excluir usuario

**DELETE** `/users/{id}`

Resposta:

- **204 No Content** quando removido
- **404 Not Found** quando o usuario nao existir
- **500 Internal Server Error** em caso de falha no banco

## Formato de erros

Erros de validacao retornam `ValidationProblem` (status 400), com mensagens por campo.

Falhas internas e de banco retornam `ProblemDetails` (status 500), por exemplo:

```json
{
  "type": "about:blank",
  "title": "Falha no banco de dados",
  "status": 500,
  "detail": "Nao foi possivel recuperar a lista de usuarios."
}
```

## Middleware

A API possui tres middlewares configurados na seguinte ordem no pipeline:

### 1. ErrorHandlingMiddleware (primeiro)

Captura qualquer excecao nao tratada em toda a aplicacao e retorna:

```json
{ "error": "Erro interno do servidor." }
```

### 2. TokenAuthenticationMiddleware (segundo)

Valida o token Bearer enviado no header `Authorization`. Retorna 401 caso ausente ou invalido:

```json
{ "error": "Nao autorizado. Token invalido ou ausente." }
```

A rota `/openapi` e isenta de autenticacao no ambiente de desenvolvimento.

### 3. RequestLoggingMiddleware (terceiro)

Registra no log de informacoes o metodo HTTP, caminho e codigo de status de cada requisicao:

```
[AUDIT] GET /users => 200
```

## Autenticacao

Todas as requisicoes devem enviar o token no header:

```
Authorization: Bearer techhive-api-key-2026
```

O token e configurado em `appsettings.json` na chave `Authentication:Token`.

## Observacoes

- O banco utilizado e `DB_TechHive` (SQL Server).
- A conexao esta configurada para `localhost` com Windows Authentication.
- No ambiente de desenvolvimento, o OpenAPI fica disponivel via `MapOpenApi()`.
