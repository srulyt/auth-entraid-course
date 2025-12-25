using System.CommandLine;
using System.CommandLine.Invocation;
using Labs.Cli.Helpers;
using Labs.Cli.Services;
using Microsoft.Identity.Client;

namespace Labs.Cli.Commands;

public static class LoginCommand
{
    public static Command Create(ConfigService configService, AuthService authService)
    {
        var command = new Command("login", "Sign in to Microsoft Entra ID");

        var modeOption = new Option<string>(
            name: "--mode",
            description: "Authentication mode: 'pkce' (default) or 'device-code'",
            getDefaultValue: () => "pkce");
        modeOption.AddAlias("-m");

        command.AddOption(modeOption);

        command.SetHandler(async (string mode) =>
        {
            try
            {
                if (!configService.IsConfigured())
                {
                    ErrorHandler.HandleConfigurationError(
                        "Application not configured. Please set Tenant ID and Client ID first.");
                    return;
                }

                ConsoleOutput.WriteHeader("Sign In to Microsoft Entra ID");

                if (mode.ToLower() == "device-code")
                {
                    ConsoleOutput.WriteInfo("Using Device Code flow");
                    Console.WriteLine("This flow is useful for devices with limited input capabilities.");
                    Console.WriteLine();

                    var result = await authService.LoginDeviceCodeAsync();

                    Console.WriteLine();
                    ConsoleOutput.WriteSuccess("Authentication successful!");
                    ConsoleOutput.WriteKeyValue("Account", result.Account.Username);
                    ConsoleOutput.WriteKeyValue("Token expires", result.ExpiresOn.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else if (mode.ToLower() == "pkce")
                {
                    ConsoleOutput.WriteInfo("Using Authorization Code + PKCE flow");
                    Console.WriteLine("Opening your default browser for authentication...");
                    Console.WriteLine();

                    var result = await authService.LoginInteractiveAsync();

                    ConsoleOutput.WriteSuccess("Authentication successful!");
                    ConsoleOutput.WriteKeyValue("Account", result.Account.Username);
                    ConsoleOutput.WriteKeyValue("Token expires", result.ExpiresOn.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
                }
                else
                {
                    ConsoleOutput.WriteError($"Invalid mode: {mode}");
                    Console.WriteLine("Valid modes are: 'pkce' or 'device-code'");
                    return;
                }

                Console.WriteLine();
                ConsoleOutput.WriteDim("Tokens are cached locally for future use.");
                ConsoleOutput.WriteDim("Run 'entra-lab account show' to view account details.");
            }
            catch (MsalException ex)
            {
                ErrorHandler.HandleMsalException(ex);
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleGeneralException(ex);
            }
        }, modeOption);

        return command;
    }
}
