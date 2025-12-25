using System.CommandLine;
using Labs.Cli.Helpers;
using Labs.Cli.Services;
using TokenInspector;

namespace Labs.Cli.Commands;

public static class TokenCommand
{
    public static Command Create(AuthService authService, TokenCacheService tokenCacheService)
    {
        var command = new Command("token", "Manage and display tokens");

        // token show
        var showCommand = new Command("show", "Display access token information");
        
        var printTokenOption = new Option<bool>(
            name: "--print-token",
            description: "Print truncated token (first/last 20 chars)",
            getDefaultValue: () => false);

        var dangerouslyPrintFullOption = new Option<bool>(
            name: "--dangerously-print-full-token",
            description: "Print the FULL token (security risk!)",
            getDefaultValue: () => false);

        showCommand.AddOption(printTokenOption);
        showCommand.AddOption(dangerouslyPrintFullOption);

        showCommand.SetHandler(async (bool printToken, bool dangerouslyPrintFull) =>
        {
            try
            {
                var token = await authService.GetAccessTokenAsync();

                if (string.IsNullOrEmpty(token))
                {
                    ConsoleOutput.WriteWarning("No token available. Please sign in first.");
                    Console.WriteLine();
                    Console.WriteLine("Run: entra-lab login --mode pkce");
                    return;
                }

                ConsoleOutput.WriteHeader("Access Token Information");

                var parts = JwtTools.DecodeToken(token);

                ConsoleOutput.WriteSubHeader("Key Claims");
                ConsoleOutput.WriteKeyValue("Audience (aud)", parts.Audiences.FirstOrDefault() ?? "N/A");
                ConsoleOutput.WriteKeyValue("Issuer (iss)", parts.Issuer);
                ConsoleOutput.WriteKeyValue("Subject (sub)", parts.Claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? "N/A");
                ConsoleOutput.WriteKeyValue("Valid From", parts.ValidFrom.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
                ConsoleOutput.WriteKeyValue("Valid To", parts.ValidTo.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
                
                var isExpired = JwtTools.IsExpired(token);
                var timeToExpiry = JwtTools.GetTimeToExpiration(token);
                
                if (isExpired)
                {
                    ConsoleOutput.WriteKeyValue("Status", "EXPIRED", 20);
                }
                else
                {
                    ConsoleOutput.WriteKeyValue("Expires In", $"{timeToExpiry.TotalMinutes:F0} minutes");
                }

                var scopes = parts.Claims.FirstOrDefault(c => c.Type == "scp")?.Value;
                if (!string.IsNullOrEmpty(scopes))
                {
                    ConsoleOutput.WriteKeyValue("Scopes (scp)", scopes);
                }

                Console.WriteLine();

                if (dangerouslyPrintFull)
                {
                    ConsoleOutput.WriteWarning("⚠ FULL TOKEN (DO NOT SHARE!)");
                    Console.WriteLine();
                    Console.WriteLine(token);
                    Console.WriteLine();
                }
                else if (printToken)
                {
                    ConsoleOutput.WriteSubHeader("Truncated Token");
                    var truncated = token.Length > 40 
                        ? $"{token.Substring(0, 20)}...{token.Substring(token.Length - 20)}"
                        : token;
                    Console.WriteLine(truncated);
                    Console.WriteLine();
                    ConsoleOutput.WriteDim("Use --dangerously-print-full-token to see the full token (not recommended)");
                }

                ConsoleOutput.WriteDim("To decode the full token, visit: https://jwt.ms");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleGeneralException(ex);
            }
        }, printTokenOption, dangerouslyPrintFullOption);

        // token clear
        var clearCommand = new Command("clear", "Clear cached tokens");
        clearCommand.SetHandler(() =>
        {
            try
            {
                tokenCacheService.ClearCache();
                ConsoleOutput.WriteSuccess("Token cache cleared.");
                ConsoleOutput.WriteDim("You will need to sign in again to acquire new tokens.");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleGeneralException(ex);
            }
        });

        command.AddCommand(showCommand);
        command.AddCommand(clearCommand);

        return command;
    }
}
