using System.CommandLine;
using Labs.Cli.Helpers;
using Labs.Cli.Services;

namespace Labs.Cli.Commands;

public static class LogoutCommand
{
    public static Command Create(AuthService authService)
    {
        var command = new Command("logout", "Sign out and clear cached tokens");

        command.SetHandler(async () =>
        {
            try
            {
                var account = await authService.GetCurrentAccountAsync();
                
                if (account == null)
                {
                    ConsoleOutput.WriteWarning("No account is currently signed in.");
                    return;
                }

                var username = account.Username;
                await authService.LogoutAsync();

                ConsoleOutput.WriteSuccess($"Signed out: {username}");
                ConsoleOutput.WriteDim("Token cache has been cleared.");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleGeneralException(ex);
            }
        });

        return command;
    }
}
