namespace Labs.Shared.Models;

/// <summary>
/// Azure AD configuration settings
/// </summary>
public class AzureAdSettings
{
    public string Instance { get; set; } = "https://login.microsoftonline.com/";
    public string Domain { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string CallbackPath { get; set; } = "/signin-oidc";
}

/// <summary>
/// Downstream API configuration
/// </summary>
public class DownstreamApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string[] Scopes { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Microsoft Graph configuration
/// </summary>
public class GraphSettings
{
    public string BaseUrl { get; set; } = "https://graph.microsoft.com/v1.0";
    public string[] Scopes { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Cross-tenant daemon configuration
/// </summary>
public class DaemonSettings
{
    public string HomeTenantId { get; set; } = string.Empty;
    public string CustomerTenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}
