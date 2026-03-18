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
- **400 Bad Request** se algum campo obrigatorio vier vazio

Campos obrigatorios:

- `fullName`
- `email`
- `department`
- `role`

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
- **400 Bad Request** se algum campo obrigatorio vier vazio
- **404 Not Found** quando o usuario nao existir

### 5. Excluir usuario

**DELETE** `/users/{id}`

Resposta:

- **204 No Content** quando removido
- **404 Not Found** quando o usuario nao existir

## Observacoes

- O banco utilizado e `DB_TechHive` (SQL Server).
- A conexao esta configurada para `localhost` com Windows Authentication.
- No ambiente de desenvolvimento, o OpenAPI fica disponivel via `MapOpenApi()`.
