using UserManagementAPI.Models;
using UserManagementAPI.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpsRedirection(options =>
{
    options.HttpsPort = 7133;
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddSingleton(new UserRepository(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// ── Users CRUD ──────────────────────────────────────────────────────────────

// GET /users  – lista todos os usuários
app.MapGet("/users", async (UserRepository repo) =>
{
    var users = await repo.GetAllAsync();
    return Results.Ok(users);
})
.WithName("GetUsers")
.WithSummary("Retorna a lista de todos os usuários");

// GET /users/{id}  – retorna um usuário pelo ID
app.MapGet("/users/{id:int}", async (int id, UserRepository repo) =>
{
    var user = await repo.GetByIdAsync(id);
    return user is not null ? Results.Ok(user) : Results.NotFound($"Usuário com Id {id} não encontrado.");
})
.WithName("GetUserById")
.WithSummary("Retorna um usuário pelo ID");

// POST /users  – cria um novo usuário
app.MapPost("/users", async (CreateUserRequest request, UserRepository repo) =>
{
    if (string.IsNullOrWhiteSpace(request.FullName) ||
        string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.Department) ||
        string.IsNullOrWhiteSpace(request.Role))
    {
        return Results.BadRequest("FullName, Email, Department e Role são obrigatórios.");
    }

    var created = await repo.CreateAsync(request);
    return Results.Created($"/users/{created.UserId}", created);
})
.WithName("CreateUser")
.WithSummary("Cria um novo usuário");

// PUT /users/{id}  – atualiza os dados de um usuário
app.MapPut("/users/{id:int}", async (int id, UpdateUserRequest request, UserRepository repo) =>
{
    if (string.IsNullOrWhiteSpace(request.FullName) ||
        string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.Department) ||
        string.IsNullOrWhiteSpace(request.Role))
    {
        return Results.BadRequest("FullName, Email, Department e Role são obrigatórios.");
    }

    var updated = await repo.UpdateAsync(id, request);
    return updated is not null ? Results.Ok(updated) : Results.NotFound($"Usuário com Id {id} não encontrado.");
})
.WithName("UpdateUser")
.WithSummary("Atualiza os dados de um usuário existente");

// DELETE /users/{id}  – remove um usuário
app.MapDelete("/users/{id:int}", async (int id, UserRepository repo) =>
{
    var deleted = await repo.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound($"Usuário com Id {id} não encontrado.");
})
.WithName("DeleteUser")
.WithSummary("Remove um usuário pelo ID");

app.Run();
