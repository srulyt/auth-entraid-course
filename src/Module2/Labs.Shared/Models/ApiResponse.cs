namespace Labs.Shared.Models;

/// <summary>
/// Standard API response wrapper
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}

/// <summary>
/// Token claims response
/// </summary>
public class TokenClaimsResponse
{
    public string TokenType { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string? Scope { get; set; }
    public string[]? Roles { get; set; }
    public string? ObjectId { get; set; }
    public string? TenantId { get; set; }
    public string? PreferredUserName { get; set; }
    public Dictionary<string, string> AllClaims { get; set; } = new();
}

/// <summary>
/// User profile from Graph
/// </summary>
public class UserProfile
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string UserPrincipalName { get; set; } = string.Empty;
    public string? Mail { get; set; }
    public string? JobTitle { get; set; }
}
