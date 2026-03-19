namespace UserManagementAPI.Middleware;

public class TokenAuthenticationMiddleware(RequestDelegate next, IConfiguration configuration)
{
    // Caminho do OpenAPI fica isento de autenticacao no ambiente de desenvolvimento
    private static readonly string[] PublicPaths = ["/openapi"];

    public async Task InvokeAsync(HttpContext context)
    {
        if (IsPublicPath(context.Request.Path))
        {
            await next(context);
            return;
        }

        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        var validToken = configuration["Authentication:Token"];

        if (string.IsNullOrEmpty(validToken) || !IsValidBearerToken(authHeader, validToken))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { error = "Nao autorizado. Token invalido ou ausente." });
            return;
        }

        await next(context);
    }

    private static bool IsPublicPath(PathString requestPath)
        => PublicPaths.Any(p => requestPath.StartsWithSegments(p));

    private static bool IsValidBearerToken(string? authHeader, string validToken)
    {
        if (string.IsNullOrEmpty(authHeader)) return false;
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return false;

        var token = authHeader["Bearer ".Length..].Trim();
        return string.Equals(token, validToken, StringComparison.Ordinal);
    }
}
