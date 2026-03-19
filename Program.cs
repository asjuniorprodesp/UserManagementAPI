using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Caching.Memory;
using UserManagementAPI.Exceptions;
using UserManagementAPI.Middleware;
using UserManagementAPI.Models;
using UserManagementAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

const string UserManagementUiCorsPolicy = "UserManagementUI";

builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.AddCors(options =>
{
    options.AddPolicy(UserManagementUiCorsPolicy, policy =>
    {
        policy
            .WithOrigins("https://localhost:7218", "http://localhost:5254")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 7133;
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddSingleton(new UserRepository(connectionString));

var app = builder.Build();

// ── Pipeline de middleware (ordem exigida) ───────────────────────────────────
// 1. Tratamento de erros – envolve tudo, captura qualquer excecao nao tratada
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseCors(UserManagementUiCorsPolicy);

// 2. Autenticacao – valida token antes de qualquer logica de negocio
app.UseMiddleware<TokenAuthenticationMiddleware>();

// 3. Registro de logs – registra metodo, caminho e status de cada requisicao
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

const string UsersListCacheKey = "users:list";
const int UsersListCacheSeconds = 30;

// ── Users CRUD ──────────────────────────────────────────────────────────────

// GET /users  – lista todos os usuários
app.MapGet("/users", async (UserRepository repo, IMemoryCache cache) =>
{
    if (cache.TryGetValue(UsersListCacheKey, out IReadOnlyCollection<User>? cachedUsers) && cachedUsers is not null)
    {
        return Results.Ok(cachedUsers);
    }

    try
    {
        var users = (await repo.GetAllAsync()).ToList();
        cache.Set(UsersListCacheKey, users, TimeSpan.FromSeconds(UsersListCacheSeconds));
        return Results.Ok(users);
    }
    catch (DatabaseOperationException)
    {
        return Results.Problem(
            title: "Falha no banco de dados",
            detail: "Nao foi possivel recuperar a lista de usuarios.",
            statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("GetUsers")
.WithSummary("Retorna a lista de todos os usuários");

// GET /users/{id}  – retorna um usuário pelo ID
app.MapGet("/users/{id:int}", async (int id, UserRepository repo) =>
{
    try
    {
        var user = await repo.GetByIdAsync(id);
        return user is not null ? Results.Ok(user) : Results.NotFound($"Usuario com Id {id} nao encontrado.");
    }
    catch (DatabaseOperationException)
    {
        return Results.Problem(
            title: "Falha no banco de dados",
            detail: "Nao foi possivel recuperar o usuario solicitado.",
            statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("GetUserById")
.WithSummary("Retorna um usuário pelo ID");

// POST /users  – cria um novo usuário
app.MapPost("/users", async (CreateUserRequest request, UserRepository repo, IMemoryCache cache) =>
{
    var validationErrors = ValidateRequest(request);
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    try
    {
        var created = await repo.CreateAsync(request);
        cache.Remove(UsersListCacheKey);
        return Results.Created($"/users/{created.UserId}", created);
    }
    catch (DatabaseOperationException)
    {
        return Results.Problem(
            title: "Falha no banco de dados",
            detail: "Nao foi possivel criar o usuario.",
            statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("CreateUser")
.WithSummary("Cria um novo usuário");

// PUT /users/{id}  – atualiza os dados de um usuário
app.MapPut("/users/{id:int}", async (int id, UpdateUserRequest request, UserRepository repo, IMemoryCache cache) =>
{
    var validationErrors = ValidateRequest(request);
    if (validationErrors.Count > 0)
    {
        return Results.ValidationProblem(validationErrors);
    }

    try
    {
        var updated = await repo.UpdateAsync(id, request);
        if (updated is null)
        {
            return Results.NotFound($"Usuario com Id {id} nao encontrado.");
        }

        cache.Remove(UsersListCacheKey);
        return Results.Ok(updated);
    }
    catch (DatabaseOperationException)
    {
        return Results.Problem(
            title: "Falha no banco de dados",
            detail: "Nao foi possivel atualizar o usuario.",
            statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("UpdateUser")
.WithSummary("Atualiza os dados de um usuário existente");

// DELETE /users/{id}  – remove um usuário
app.MapDelete("/users/{id:int}", async (int id, UserRepository repo, IMemoryCache cache) =>
{
    try
    {
        var deleted = await repo.DeleteAsync(id);
        if (!deleted)
        {
            return Results.NotFound($"Usuario com Id {id} nao encontrado.");
        }

        cache.Remove(UsersListCacheKey);
        return Results.NoContent();
    }
    catch (DatabaseOperationException)
    {
        return Results.Problem(
            title: "Falha no banco de dados",
            detail: "Nao foi possivel excluir o usuario.",
            statusCode: StatusCodes.Status500InternalServerError);
    }
})
.WithName("DeleteUser")
.WithSummary("Remove um usuário pelo ID");

app.Run();

static Dictionary<string, string[]> ValidateRequest<T>(T request)
{
    var context = new ValidationContext(request!);
    var results = new List<ValidationResult>();
    Validator.TryValidateObject(request!, context, results, validateAllProperties: true);

    if (request is CreateUserRequest createRequest)
    {
        AddWhitespaceValidationErrors(results, createRequest.FullName, nameof(CreateUserRequest.FullName));
        AddWhitespaceValidationErrors(results, createRequest.Email, nameof(CreateUserRequest.Email));
        AddWhitespaceValidationErrors(results, createRequest.Department, nameof(CreateUserRequest.Department));
        AddWhitespaceValidationErrors(results, createRequest.Role, nameof(CreateUserRequest.Role));
    }

    if (request is UpdateUserRequest updateRequest)
    {
        AddWhitespaceValidationErrors(results, updateRequest.FullName, nameof(UpdateUserRequest.FullName));
        AddWhitespaceValidationErrors(results, updateRequest.Email, nameof(UpdateUserRequest.Email));
        AddWhitespaceValidationErrors(results, updateRequest.Department, nameof(UpdateUserRequest.Department));
        AddWhitespaceValidationErrors(results, updateRequest.Role, nameof(UpdateUserRequest.Role));
    }

    return results
        .SelectMany(validationResult => validationResult.MemberNames.DefaultIfEmpty(string.Empty),
            (validationResult, memberName) => new { memberName, validationResult.ErrorMessage })
        .GroupBy(x => string.IsNullOrWhiteSpace(x.memberName) ? "request" : x.memberName)
        .ToDictionary(
            group => group.Key,
            group => group
                .Select(x => x.ErrorMessage ?? "Campo invalido.")
                .Distinct()
                .ToArray());
}

static void AddWhitespaceValidationErrors(List<ValidationResult> results, string value, string fieldName)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        results.Add(new ValidationResult($"O campo {fieldName} nao pode ser vazio.", [fieldName]));
    }
}
