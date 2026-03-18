using Dapper;
using Microsoft.Data.SqlClient;
using UserManagementAPI.Exceptions;
using UserManagementAPI.Models;

namespace UserManagementAPI.Repositories;

public class UserRepository(string connectionString)
{
    private SqlConnection CreateConnection() => new(connectionString);
    private const string UserColumns = "UserId, FullName, Email, Department, Role, IsActive, CreatedAt, UpdatedAt";

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        string sql = $"SELECT {UserColumns} FROM dbo.Users ORDER BY UserId";
        try
        {
            using var conn = CreateConnection();
            return await conn.QueryAsync<User>(sql);
        }
        catch (SqlException ex)
        {
            throw new DatabaseOperationException("Erro ao buscar lista de usuarios.", ex);
        }
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        string sql = $"SELECT {UserColumns} FROM dbo.Users WHERE UserId = @UserId";
        try
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new { UserId = id });
        }
        catch (SqlException ex)
        {
            throw new DatabaseOperationException("Erro ao buscar usuario por ID.", ex);
        }
    }

    public async Task<User> CreateAsync(CreateUserRequest request)
    {
        const string sql = """
            INSERT INTO dbo.Users (FullName, Email, Department, Role, IsActive)
            OUTPUT INSERTED.*
            VALUES (@FullName, @Email, @Department, @Role, @IsActive)
            """;
        try
        {
            using var conn = CreateConnection();
            return await conn.QuerySingleAsync<User>(sql, request);
        }
        catch (SqlException ex)
        {
            throw new DatabaseOperationException("Erro ao criar usuario.", ex);
        }
    }

    public async Task<User?> UpdateAsync(int id, UpdateUserRequest request)
    {
        const string sql = """
            UPDATE dbo.Users
            SET FullName   = @FullName,
                Email      = @Email,
                Department = @Department,
                Role       = @Role,
                IsActive   = @IsActive,
                UpdatedAt  = SYSUTCDATETIME()
            OUTPUT INSERTED.*
            WHERE UserId = @UserId
            """;
        try
        {
            using var conn = CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new
            {
                request.FullName,
                request.Email,
                request.Department,
                request.Role,
                request.IsActive,
                UserId = id
            });
        }
        catch (SqlException ex)
        {
            throw new DatabaseOperationException("Erro ao atualizar usuario.", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM dbo.Users WHERE UserId = @UserId";
        try
        {
            using var conn = CreateConnection();
            int rows = await conn.ExecuteAsync(sql, new { UserId = id });
            return rows > 0;
        }
        catch (SqlException ex)
        {
            throw new DatabaseOperationException("Erro ao excluir usuario.", ex);
        }
    }
}
