using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using UserManagementUI.Models;

namespace UserManagementUI.Services;

public class UserApiClient(HttpClient httpClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<UserDto>>("users", cancellationToken) ?? [];
    }

    public async Task<UserDto> CreateUserAsync(UserRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("users", request, cancellationToken);
        return await ReadResponseAsync<UserDto>(response, cancellationToken);
    }

    public async Task<UserDto> UpdateUserAsync(int userId, UserRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync($"users/{userId}", request, cancellationToken);
        return await ReadResponseAsync<UserDto>(response, cancellationToken);
    }

    public async Task DeleteUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.DeleteAsync($"users/{userId}", cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        throw await CreateApiExceptionAsync(response, cancellationToken);
    }

    private static async Task<T> ReadResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);
            return data ?? throw new ApiClientException("A API retornou uma resposta vazia.", response.StatusCode);
        }

        throw await CreateApiExceptionAsync(response, cancellationToken);
    }

    private static async Task<ApiClientException> CreateApiExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(content))
        {
            try
            {
                var problem = JsonSerializer.Deserialize<ApiProblemDetails>(content, JsonOptions);
                if (problem?.Errors is { Count: > 0 })
                {
                    var message = string.Join(" ", problem.Errors.SelectMany(entry => entry.Value).Distinct());
                    return new ApiClientException(message, response.StatusCode);
                }

                if (!string.IsNullOrWhiteSpace(problem?.Detail))
                {
                    return new ApiClientException(problem.Detail, response.StatusCode);
                }

                if (!string.IsNullOrWhiteSpace(problem?.Title))
                {
                    return new ApiClientException(problem.Title, response.StatusCode);
                }
            }
            catch (JsonException)
            {
            }

            try
            {
                using var jsonDocument = JsonDocument.Parse(content);
                if (jsonDocument.RootElement.TryGetProperty("error", out var errorElement))
                {
                    var errorMessage = errorElement.GetString();
                    if (!string.IsNullOrWhiteSpace(errorMessage))
                    {
                        return new ApiClientException(errorMessage, response.StatusCode);
                    }
                }
            }
            catch (JsonException)
            {
            }

            var plainText = content.Trim().Trim('"');
            if (!string.IsNullOrWhiteSpace(plainText))
            {
                return new ApiClientException(plainText, response.StatusCode);
            }
        }

        return new ApiClientException($"Falha na chamada da API. Status: {(int)response.StatusCode}.", response.StatusCode);
    }
}

public class ApiClientException(string message, HttpStatusCode statusCode) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}