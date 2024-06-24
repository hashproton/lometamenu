using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.Errors;
using Application.Errors.Common;
using Application.Services;
using Application.UseCases.Tenants.Commands;

namespace Infra.Services;

public class Claim
{
    public string Type { get; set; }

    public string Value { get; set; }
}

internal sealed class RawGetMeResponse
{
    public Guid UserId { get; set; }

    public string Email { get; set; }

    public List<string> Roles { get; set; }

    public List<Claim> Claims { get; set; }
}

public class AuthService(IHttpClientFactory httpClientFactory) : IAuthService
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("identipass");

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var content = new StringContent(JsonSerializer.Serialize(request),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);
        var response = await _httpClient.PostAsync("api/auth/login", content, cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, _jsonSerializerOptions);
            if (loginResponse is null)
            {
                return Result.Failure<LoginResponse>(AuthServiceErrors.InvalidResponse);
            }

            return Result.Success(loginResponse!);
        }

        var errorResponse = JsonSerializer.Deserialize<ErrorsResponse>(responseContent, _jsonSerializerOptions);

        var firstError = errorResponse!.Errors.First();

        var error = new Error(firstError.Code, firstError.Message, firstError.Type);

        return Result.Failure<LoginResponse>(error);
    }

    public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var content = new StringContent(JsonSerializer.Serialize(request),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);
        var response = await _httpClient.PostAsync("api/auth/register", content, cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return Result.Success();
        }

        var errorResponse = JsonSerializer.Deserialize<ErrorsResponse>(responseContent, _jsonSerializerOptions);
        if (errorResponse is null)
        {
            return Result.Failure(AuthServiceErrors.InvalidResponse);
        }

        var firstError = errorResponse.Errors.First();
        var error = new Error(firstError.Code, firstError.Message, firstError.Type);

        return Result.Failure(error);
    }

    public async Task<Result<GetMeResponse>> GetMeAsync(
        string token,
        string refreshToken,
        CancellationToken cancellationToken)
    {
        if (token.StartsWith("Bearer "))
        {
            token = token.Substring("Bearer ".Length);
        }

        var request = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
        request.Headers.Add("Authorization", token);
        request.Headers.Add("RefreshToken", refreshToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var rawGetMeResponse
                = JsonSerializer.Deserialize<RawGetMeResponse>(responseContent, _jsonSerializerOptions);
            if (rawGetMeResponse is null)
            {
                return Result.Failure<GetMeResponse>(AuthServiceErrors.InvalidResponse);
            }

            var mappedGetMeResponse = new GetMeResponse
            {
                Id = rawGetMeResponse.UserId,
                Email = rawGetMeResponse.Email,
                Roles = rawGetMeResponse.Roles
                    .Select(r => Enum.Parse<Role>(r.RemoveWhitespace()))
                    .ToList(),
                TenantIds = rawGetMeResponse.Claims
                    .Where(c => c.Type == "tenant")
                    .Select(c => int.Parse(c.Value))
                    .ToList(),
            };

            return Result.Success(mappedGetMeResponse);
        }

        var errorResponse = JsonSerializer.Deserialize<ErrorsResponse>(responseContent, _jsonSerializerOptions);

        var firstError = errorResponse.Errors.First();
        var error = new Error(firstError.Code, firstError.Message, firstError.Type);

        return Result.Failure<GetMeResponse>(error);
    }
}

public static class StringExtensions
{
    public static string RemoveWhitespace(this string input)
    {
        return new string(input.ToCharArray()
            .Where(c => !char.IsWhiteSpace(c))
            .ToArray());
    }
}