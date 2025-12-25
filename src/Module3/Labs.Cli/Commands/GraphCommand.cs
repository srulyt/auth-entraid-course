using System.CommandLine;
using Labs.Cli.Helpers;
using Labs.Cli.Services;

namespace Labs.Cli.Commands;

public static class GraphCommand
{
    public static Command Create(GraphService graphService)
    {
        var command = new Command("graph", "Call Microsoft Graph API");

        // graph me
        var meCommand = new Command("me", "Get current user's profile from Microsoft Graph");
        meCommand.SetHandler(async () =>
        {
            try
            {
                ConsoleOutput.WriteInfo("Calling Microsoft Graph API: /me");
                Console.WriteLine();

                var profile = await graphService.GetMyProfileAsync();

                if (profile == null)
                {
                    ConsoleOutput.WriteWarning("Unable to retrieve profile. Please sign in first.");
                    Console.WriteLine();
                    Console.WriteLine("Run: entra-lab login --mode pkce");
                    return;
                }

                ConsoleOutput.WriteHeader("User Profile from Microsoft Graph");
                
                if (!string.IsNullOrEmpty(profile.DisplayName))
                    ConsoleOutput.WriteKeyValue("Display Name", profile.DisplayName);
                
                if (!string.IsNullOrEmpty(profile.UserPrincipalName))
                    ConsoleOutput.WriteKeyValue("UPN", profile.UserPrincipalName);
                
                if (!string.IsNullOrEmpty(profile.Mail))
                    ConsoleOutput.WriteKeyValue("Email", profile.Mail);
                
                if (!string.IsNullOrEmpty(profile.JobTitle))
                    ConsoleOutput.WriteKeyValue("Job Title", profile.JobTitle);
                
                if (!string.IsNullOrEmpty(profile.MobilePhone))
                    ConsoleOutput.WriteKeyValue("Mobile Phone", profile.MobilePhone);
                
                if (!string.IsNullOrEmpty(profile.OfficeLocation))
                    ConsoleOutput.WriteKeyValue("Office Location", profile.OfficeLocation);
                
                if (!string.IsNullOrEmpty(profile.PreferredLanguage))
                    ConsoleOutput.WriteKeyValue("Language", profile.PreferredLanguage);
                
                if (!string.IsNullOrEmpty(profile.Id))
                    ConsoleOutput.WriteKeyValue("Object ID", profile.Id);

                Console.WriteLine();
                ConsoleOutput.WriteSuccess("Graph API call successful!");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleGeneralException(ex);
            }
        });

        // graph scopes
        var scopesCommand = new Command("scopes", "Show Microsoft Graph scopes being requested");
        scopesCommand.SetHandler(() =>
        {
            try
            {
                var scopes = graphService.GetRequestedScopes();

                ConsoleOutput.WriteHeader("Microsoft Graph Scopes");
                Console.WriteLine("The following delegated permissions are requested:");
                Console.WriteLine();

                foreach (var scope in scopes)
                {
                    ConsoleOutput.WriteKeyValue("", scope, 2);
                }

                Console.WriteLine();
                ConsoleOutput.WriteDim("These scopes are configured in appsettings.json under Graph:Scopes");
                ConsoleOutput.WriteDim("Users must consent to these permissions during sign-in");
            }
            catch (Exception ex)
            {
                ErrorHandler.HandleGeneralException(ex);
            }
        });

        command.AddCommand(meCommand);
        command.AddCommand(scopesCommand);

        return command;
    }
}
