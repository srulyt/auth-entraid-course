using System.CommandLine;
using Labs.Cli.Helpers;
using Labs.Cli.Services;

namespace Labs.Cli.Commands;

public static class ConfigCommand
{
    public static Command Create(ConfigService configService)
    {
        var command = new Command("config", "Manage application configuration");

        // config show
        var showCommand = new Command("show", "Show current configuration");
        showCommand.SetHandler(() =>
        {
            try
            {
                var config = configService.GetConfig();
                var isConfigured = configService.IsConfigured();

                ConsoleOutput.WriteHeader("Application Configuration");

                if (isConfigured)
                {
                    ConsoleOutput.WriteSuccess("Configuration is valid");
                }
                else
                {
                    ConsoleOutput.WriteWarning("Configuration is incomplete");
                }

                Console.WriteLine();
                ConsoleOutput.WriteKeyValue("Tenant ID", config.TenantId);
                ConsoleOutput.WriteKeyValue("Client ID", config.ClientId);
                ConsoleOutput.WriteKeyValue("Redirect URI", config.RedirectUri);
                ConsoleOutput.WriteKeyValue("Graph Base URL", config.GraphBaseUrl);
                ConsoleOutput.WriteKeyValue("Graph Scopes", string.Join(", ", config.GraphScopes));

                Console.WriteLine();
                
                if (!isConfigured)
                {
                    Console.WriteLine("To configure, run:");
                    Console.WriteLine("  entra-lab config set --tenant <tenant-id> --client <client-id>");
                    Console.WriteLine();
                    Console.WriteLine("Or edit appsettings.json directly.");
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleGeneralException(ex);
            }
        });

        // config set
        var setCommand = new Command("set", "Set configuration values");

        var tenantOption = new Option<string?>(
            name: "--tenant",
            description: "Tenant ID (Directory ID)");
        tenantOption.AddAlias("-t");

        var clientOption = new Option<string?>(
            name: "--client",
            description: "Client ID (Application ID)");
        clientOption.AddAlias("-c");

        setCommand.AddOption(tenantOption);
        setCommand.AddOption(clientOption);

        setCommand.SetHandler((string? tenantId, string? clientId) =>
        {
            try
            {
                if (string.IsNullOrEmpty(tenantId) && string.IsNullOrEmpty(clientId))
                {
                    ConsoleOutput.WriteError("At least one configuration value must be provided.");
                    Console.WriteLine();
                    Console.WriteLine("Usage: entra-lab config set --tenant <id> --client <id>");
                    return;
                }

                configService.UpdateConfig(tenantId, clientId);

                ConsoleOutput.WriteSuccess("Configuration updated successfully");
                Console.WriteLine();

                var config = configService.GetConfig();
                ConsoleOutput.WriteKeyValue("Tenant ID", config.TenantId);
                ConsoleOutput.WriteKeyValue("Client ID", config.ClientId);

                Console.WriteLine();
                ConsoleOutput.WriteDim("Configuration saved to appsettings.json");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleGeneralException(ex);
            }
        }, tenantOption, clientOption);

        command.AddCommand(showCommand);
        command.AddCommand(setCommand);

        return command;
    }
}
