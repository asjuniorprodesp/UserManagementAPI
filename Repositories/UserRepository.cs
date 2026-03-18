using Dapper;
using Microsoft.Data.SqlClient;
using UserManagementAPI.Models;

namespace UserManagementAPI.Repositories;

public class UserRepository(string connectionString)
{
    private SqlConnection CreateConnection() => new(connectionString);

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        const string sql = "SELECT * FROM dbo.Users ORDER BY UserId";
        using var conn = CreateConnection();
        return await conn.QueryAsync<User>(sql);
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM dbo.Users WHERE UserId = @UserId";
        using var conn = CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<User>(sql, new { UserId = id });
    }

    public async Task<User> CreateAsync(CreateUserRequest request)
    {
        const string sql = """
            INSERT INTO dbo.Users (FullName, Email, Department, Role, IsActive)
            OUTPUT INSERTED.*
            VALUES (@FullName, @Email, @Department, @Role, @IsActive)
            """;
        using var conn = CreateConnection();
        return await conn.QuerySingleAsync<User>(sql, request);
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

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM dbo.Users WHERE UserId = @UserId";
        using var conn = CreateConnection();
        int rows = await conn.ExecuteAsync(sql, new { UserId = id });
        return rows > 0;
    }
}
