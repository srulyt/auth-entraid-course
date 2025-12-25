using System.CommandLine;
using Labs.Cli.Helpers;
using Labs.Cli.Services;

namespace Labs.Cli.Commands;

public static class AccountCommand
{
    public static Command Create(AuthService authService)
    {
        var command = new Command("account", "Manage account information");

        var showCommand = new Command("show", "Show current signed-in account");
        showCommand.SetHandler(async () =>
        {
            try
            {
                var account = await authService.GetCurrentAccountAsync();

                if (account == null)
                {
                    ConsoleOutput.WriteWarning("No account is currently signed in.");
                    Console.WriteLine();
                    Console.WriteLine("To sign in, run:");
                    Console.WriteLine("  entra-lab login --mode pkce");
                    Console.WriteLine("  entra-lab login --mode device-code");
                    return;
                }

                ConsoleOutput.WriteHeader("Current Account");
                ConsoleOutput.WriteKeyValue("Username", account.Username);
                ConsoleOutput.WriteKeyValue("Name", account.Username.Split('@')[0]);
                ConsoleOutput.WriteKeyValue("Home Account ID", account.HomeAccountId.Identifier);
                
                if (account.Environment != null)
                {
                    ConsoleOutput.WriteKeyValue("Environment", account.Environment);
                }

                Console.WriteLine();
                ConsoleOutput.WriteDim("To sign out: entra-lab logout");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleGeneralException(ex);
            }
        });

        command.AddCommand(showCommand);
        return command;
    }
}
