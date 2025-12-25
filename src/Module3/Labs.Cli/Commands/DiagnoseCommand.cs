using System.CommandLine;
using Labs.Cli.Helpers;
using Labs.Cli.Services;

namespace Labs.Cli.Commands;

public static class DiagnoseCommand
{
    public static Command Create(ConfigService configService, TokenCacheService tokenCacheService, AuthService authService)
    {
        var command = new Command("diagnose", "Check configuration and connectivity");

        command.SetHandler(async () =>
        {
            try
            {
                ConsoleOutput.WriteHeader("Diagnostics");

                // Check 1: Configuration
                ConsoleOutput.WriteSubHeader("1. Configuration Check");
                var config = configService.GetConfig();
                var isConfigured = configService.IsConfigured();

                if (isConfigured)
                {
                    ConsoleOutput.WriteSuccess("Configuration is valid");
                    ConsoleOutput.WriteKeyValue("  Tenant ID", config.TenantId);
                    ConsoleOutput.WriteKeyValue("  Client ID", config.ClientId);
                }
                else
                {
                    ConsoleOutput.WriteError("Configuration is incomplete");
                    ConsoleOutput.WriteKeyValue("  Tenant ID", config.TenantId);
                    ConsoleOutput.WriteKeyValue("  Client ID", config.ClientId);
                    Console.WriteLine();
                    Console.WriteLine("  Fix: Run 'entra-lab config set --tenant <id> --client <id>'");
                }

                Console.WriteLine();

                // Check 2: Token Cache
                ConsoleOutput.WriteSubHeader("2. Token Cache Check");
                var cacheExists = tokenCacheService.CacheExists();
                var cacheLocation = tokenCacheService.GetCacheLocation();

                ConsoleOutput.WriteKeyValue("  Cache Location", cacheLocation);

                if (cacheExists)
                {
                    var cacheSize = tokenCacheService.GetCacheSize();
                    ConsoleOutput.WriteSuccess($"Cache exists ({cacheSize} bytes)");
                }
                else
                {
                    ConsoleOutput.WriteWarning("No cached tokens found");
                    Console.WriteLine("  Info: This is normal if you haven't signed in yet");
                }

                Console.WriteLine();

                // Check 3: Authentication Status
                ConsoleOutput.WriteSubHeader("3. Authentication Status");
                var account = await authService.GetCurrentAccountAsync();

                if (account != null)
                {
                    ConsoleOutput.WriteSuccess($"Signed in as {account.Username}");
                    
                    var token = await authService.GetAccessTokenAsync();
                    if (token != null)
                    {
                        ConsoleOutput.WriteSuccess("Valid access token available");
                    }
                    else
                    {
                        ConsoleOutput.WriteWarning("No access token (may need to sign in again)");
                    }
                }
                else
                {
                    ConsoleOutput.WriteWarning("Not signed in");
                    Console.WriteLine("  To sign in: entra-lab login --mode pkce");
                }

                Console.WriteLine();

                // Check 4: Network Connectivity
                ConsoleOutput.WriteSubHeader("4. Network Connectivity");
                try
                {
                    using var httpClient = new HttpClient();
                    httpClient.Timeout = TimeSpan.FromSeconds(5);
                    var response = await httpClient.GetAsync("https://login.microsoftonline.com");
                    
                    if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Redirect)
                    {
                        ConsoleOutput.WriteSuccess("Can reach login.microsoftonline.com");
                    }
                    else
                    {
                        ConsoleOutput.WriteWarning($"Unexpected response from login.microsoftonline.com: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    ConsoleOutput.WriteError($"Cannot reach login.microsoftonline.com: {ex.Message}");
                    Console.WriteLine("  Check your internet connection and firewall settings");
                }

                Console.WriteLine();

                // Summary
                ConsoleOutput.WriteSubHeader("Summary");
                if (isConfigured && account != null)
                {
                    ConsoleOutput.WriteSuccess("All checks passed! Ready to use.");
                }
                else if (isConfigured)
                {
                    ConsoleOutput.WriteWarning("Configuration OK, but not signed in");
                    Console.WriteLine("  Run: entra-lab login --mode pkce");
                }
                else
                {
                    ConsoleOutput.WriteError("Configuration required");
                    Console.WriteLine("  Run: entra-lab config set --tenant <id> --client <id>");
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleGeneralException(ex);
            }
        });

        return command;
    }
}
