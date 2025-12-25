using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Labs.Cli.Services;

public class ConfigService
{
    private readonly string _configPath;
    private PublicClientConfig? _config;

    public ConfigService()
    {
        _configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        LoadConfiguration();
    }

    public PublicClientConfig GetConfig()
    {
        return _config ?? new PublicClientConfig();
    }

    public bool IsConfigured()
    {
        return _config != null &&
               !string.IsNullOrEmpty(_config.TenantId) &&
               !string.IsNullOrEmpty(_config.ClientId) &&
               _config.TenantId != "YOUR_TENANT_ID" &&
               _config.ClientId != "YOUR_CLIENT_ID";
    }

    public void UpdateConfig(string? tenantId = null, string? clientId = null)
    {
        // Load current config from file
        var json = File.ReadAllText(_configPath);
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Build updated config
        var updatedConfig = new Dictionary<string, object>();

        // Copy existing sections
        foreach (var property in root.EnumerateObject())
        {
            if (property.Name == "PublicClient")
            {
                var publicClient = new Dictionary<string, string>();
                foreach (var prop in property.Value.EnumerateObject())
                {
                    publicClient[prop.Name] = prop.Value.GetString() ?? string.Empty;
                }

                // Update with new values
                if (tenantId != null) publicClient["TenantId"] = tenantId;
                if (clientId != null) publicClient["ClientId"] = clientId;

                updatedConfig["PublicClient"] = publicClient;
            }
            else
            {
                // Copy other sections as-is
                updatedConfig[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText())!;
            }
        }

        // Write back to file
        var options = new JsonSerializerOptions { WriteIndented = true };
        var updatedJson = JsonSerializer.Serialize(updatedConfig, options);
        File.WriteAllText(_configPath, updatedJson);

        // Reload
        LoadConfiguration();
    }

    private void LoadConfiguration()
    {
        try
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            _config = new PublicClientConfig
            {
                TenantId = configuration["PublicClient:TenantId"] ?? string.Empty,
                ClientId = configuration["PublicClient:ClientId"] ?? string.Empty,
                RedirectUri = configuration["PublicClient:RedirectUri"] ?? "http://localhost",
                GraphBaseUrl = configuration["Graph:BaseUrl"] ?? "https://graph.microsoft.com/v1.0",
                GraphScopes = configuration.GetSection("Graph:Scopes").Get<string[]>() ?? new[] { "User.Read" }
            };
        }
        catch
        {
            _config = null;
        }
    }
}

public class PublicClientConfig
{
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = "http://localhost";
    public string GraphBaseUrl { get; set; } = "https://graph.microsoft.com/v1.0";
    public string[] GraphScopes { get; set; } = Array.Empty<string>();
}
