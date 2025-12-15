using Microsoft.Identity.Client;
using Microsoft.Graph;
using Microsoft.Extensions.Configuration;
using Labs.Shared;
using Labs.Shared.Models;
using TokenInspector;

Console.WriteLine("=".PadRight(80, '='));
Console.WriteLine("Lab 4: Cross-Tenant Daemon (App-Only Authentication)");
Console.WriteLine("=".PadRight(80, '='));
Console.WriteLine();

// Load configuration
var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var settings = config.GetSection("Daemon").Get<DaemonSettings>();

if (settings == null || string.IsNullOrEmpty(settings.ClientId))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("ERROR: Configuration not found. Please update appsettings.json with your values.");
    Console.ResetColor();
    return;
}

// Build confidential client application
var app = ConfidentialClientApplicationBuilder
    .Create(settings.ClientId)
    .WithClientSecret(settings.ClientSecret)
    .WithAuthority($"https://login.microsoftonline.com/{settings.HomeTenantId}")
    .Build();

bool running = true;

while (running)
{
    Console.WriteLine();
    Console.WriteLine("Choose an option:");
    Console.WriteLine("1. Acquire token for home tenant");
    Console.WriteLine("2. Acquire token for customer tenant");
    Console.WriteLine("3. Call Graph /organization (customer tenant)");
    Console.WriteLine("4. Call Graph /users?$top=1 (customer tenant)");
    Console.WriteLine("5. Display last acquired token");
    Console.WriteLine("0. Exit");
    Console.Write("> ");

    var choice = Console.ReadLine();
    string? token = null;

    try
    {
        switch (choice)
        {
            case "1":
                token = await AcquireTokenAsync(app, settings.HomeTenantId, "Home Tenant");
                break;

            case "2":
                token = await AcquireTokenAsync(app, settings.CustomerTenantId, "Customer Tenant");
                break;

            case "3":
                token = await AcquireTokenAsync(app, settings.CustomerTenantId, "Customer Tenant");
                if (token != null)
                    await CallGraphOrganizationAsync(token);
                break;

            case "4":
                token = await AcquireTokenAsync(app, settings.CustomerTenantId, "Customer Tenant");
                if (token != null)
                    await CallGraphUsersAsync(token);
                break;

            case "5":
                if (!string.IsNullOrEmpty(token))
                    DisplayToken(token);
                else
                    Console.WriteLine("No token acquired yet. Choose option 1 or 2 first.");
                break;

            case "0":
                running = false;
                break;

            default:
                Console.WriteLine("Invalid choice. Please try again.");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
    }
}

Console.WriteLine("Goodbye!");

static async Task<string?> AcquireTokenAsync(IConfidentialClientApplication app, string tenantId, string tenantName)
{
    Console.WriteLine($"\nAcquiring app-only token for {tenantName} ({tenantId})...");
    
    var scopes = new[] { "https://graph.microsoft.com/.default" };
    
    var result = await app.AcquireTokenForClient(scopes)
        .WithTenantId(tenantId)
        .ExecuteAsync();

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("✓ Token acquired successfully!");
    Console.ResetColor();
    Console.WriteLine($"  Expires: {result.ExpiresOn.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
    
    return result.AccessToken;
}

static void DisplayToken(string token)
{
    Console.WriteLine("\n" + "=".PadRight(80, '='));
    Console.WriteLine("DECODED TOKEN");
    Console.WriteLine("=".PadRight(80, '='));

    try
    {
        var parts = JwtTools.DecodeToken(token);
        
        Console.WriteLine("\n[HEADER]");
        Console.WriteLine(parts.Header);
        
        Console.WriteLine("\n[PAYLOAD]");
        Console.WriteLine(parts.Payload);
        
        Console.WriteLine("\n[KEY CLAIMS]");
        Console.WriteLine($"  Audience (aud): {parts.Audiences.FirstOrDefault()}");
        Console.WriteLine($"  Issuer (iss):   {parts.Issuer}");
        Console.WriteLine($"  Valid From:     {parts.ValidFrom.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine($"  Valid To:       {parts.ValidTo.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
        
        var roles = parts.Claims.Where(c => c.Type == "roles").Select(c => c.Value).ToArray();
        if (roles.Any())
        {
            Console.WriteLine($"  Roles:          {string.Join(", ", roles)}");
        }
        
        var tid = parts.Claims.FirstOrDefault(c => c.Type == "tid")?.Value;
        Console.WriteLine($"  Tenant ID:      {tid}");
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error decoding token: {ex.Message}");
        Console.ResetColor();
    }
}

static async Task CallGraphOrganizationAsync(string token)
{
    Console.WriteLine("\nCalling Microsoft Graph /organization...");
    
    using var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Authorization = 
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    
    var response = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/organization");
    var content = await response.Content.ReadAsStringAsync();
    
    if (response.IsSuccessStatusCode)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✓ Success!");
        Console.ResetColor();
        Console.WriteLine(content);
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"✗ Error: {response.StatusCode}");
        Console.WriteLine(content);
        Console.ResetColor();
    }
}

static async Task CallGraphUsersAsync(string token)
{
    Console.WriteLine("\nCalling Microsoft Graph /users?$top=1...");
    
    using var httpClient = new HttpClient();
    httpClient.DefaultRequestHeaders.Authorization = 
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    
    var response = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/users?$top=1");
    var content = await response.Content.ReadAsStringAsync();
    
    if (response.IsSuccessStatusCode)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✓ Success!");
        Console.ResetColor();
        Console.WriteLine(content);
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"✗ Error: {response.StatusCode}");
        Console.WriteLine(content);
        Console.ResetColor();
    }
}
