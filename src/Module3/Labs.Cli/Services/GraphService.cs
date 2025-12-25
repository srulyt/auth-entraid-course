using System.Net.Http.Headers;
using System.Text.Json;

namespace Labs.Cli.Services;

public class GraphService
{
    private readonly AuthService _authService;
    private readonly ConfigService _configService;
    private readonly HttpClient _httpClient;

    public GraphService(AuthService authService, ConfigService configService)
    {
        _authService = authService;
        _configService = configService;
        _httpClient = new HttpClient();
    }

    public async Task<GraphUserProfile?> GetMyProfileAsync()
    {
        var token = await _authService.GetAccessTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        var config = _configService.GetConfig();
        var requestUrl = $"{config.GraphBaseUrl}/me";

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var response = await _httpClient.GetAsync(requestUrl);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Graph API call failed: {response.StatusCode}\n{error}");
        }

        var json = await response.Content.ReadAsStringAsync();
        var profile = JsonSerializer.Deserialize<GraphUserProfile>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return profile;
    }

    public string[] GetRequestedScopes()
    {
        var config = _configService.GetConfig();
        return config.GraphScopes;
    }
}

public class GraphUserProfile
{
    public string? DisplayName { get; set; }
    public string? GivenName { get; set; }
    public string? Surname { get; set; }
    public string? UserPrincipalName { get; set; }
    public string? Mail { get; set; }
    public string? JobTitle { get; set; }
    public string? MobilePhone { get; set; }
    public string? OfficeLocation { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? Id { get; set; }
}
