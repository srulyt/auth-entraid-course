using Microsoft.Identity.Client;

namespace Labs.Cli.Helpers;

public static class ErrorHandler
{
    public static void HandleMsalException(MsalException ex)
    {
        ConsoleOutput.WriteError($"Authentication failed: {ex.ErrorCode}");
        Console.WriteLine();

        switch (ex.ErrorCode)
        {
            case "authentication_canceled":
                Console.WriteLine("The authentication was canceled by the user.");
                Console.WriteLine("Please try again and complete the sign-in process.");
                break;

            case "invalid_client":
                Console.WriteLine("The Client ID is invalid or the application is not properly configured.");
                Console.WriteLine("Check your configuration:");
                Console.WriteLine("  - Verify the Client ID in appsettings.json");
                Console.WriteLine("  - Ensure the app registration exists in your tenant");
                break;

            case "invalid_tenant":
                Console.WriteLine("The Tenant ID is invalid.");
                Console.WriteLine("Check your configuration:");
                Console.WriteLine("  - Verify the Tenant ID in appsettings.json");
                Console.WriteLine("  - Ensure you're using the correct tenant");
                break;

            case "unauthorized_client":
                Console.WriteLine("The application is not authorized to perform this operation.");
                Console.WriteLine("Check your app registration:");
                Console.WriteLine("  - Ensure 'Allow public client flows' is enabled for device code");
                Console.WriteLine("  - Verify the redirect URI is configured for PKCE flow");
                break;

            case "invalid_grant":
                Console.WriteLine("The authorization code or token is invalid or expired.");
                Console.WriteLine("This might happen if:");
                Console.WriteLine("  - The user's password has changed");
                Console.WriteLine("  - The token cache is corrupted");
                Console.WriteLine("Try clearing the token cache: entra-lab token clear");
                break;

            case "interaction_required":
                Console.WriteLine("User interaction is required to complete authentication.");
                Console.WriteLine("Try signing in again with: entra-lab login --mode pkce");
                break;

            case "consent_required":
                Console.WriteLine("Consent is required for the requested scopes.");
                Console.WriteLine("An admin may need to grant consent in the Azure Portal:");
                Console.WriteLine("  - Go to Azure AD > App registrations > [Your App]");
                Console.WriteLine("  - Navigate to API permissions");
                Console.WriteLine("  - Click 'Grant admin consent'");
                break;

            default:
                Console.WriteLine($"Error details: {ex.Message}");
                if (!string.IsNullOrEmpty(ex.ErrorCode))
                {
                    Console.WriteLine($"Error code: {ex.ErrorCode}");
                }
                Console.WriteLine();
                Console.WriteLine("For more information about this error, visit:");
                Console.WriteLine($"https://login.microsoftonline.com/error?code={ex.ErrorCode}");
                break;
        }

        Console.WriteLine();
        ConsoleOutput.WriteDim("Tip: Run 'entra-lab diagnose' to check your configuration.");
    }

    public static void HandleGeneralException(Exception ex)
    {
        ConsoleOutput.WriteError($"An error occurred: {ex.Message}");
        Console.WriteLine();
        ConsoleOutput.WriteDim($"Exception type: {ex.GetType().Name}");
        
        if (ex.InnerException != null)
        {
            Console.WriteLine();
            ConsoleOutput.WriteDim($"Inner exception: {ex.InnerException.Message}");
        }
    }

    public static void HandleConfigurationError(string message)
    {
        ConsoleOutput.WriteError("Configuration error");
        Console.WriteLine(message);
        Console.WriteLine();
        Console.WriteLine("Please update your configuration:");
        Console.WriteLine("  - Edit appsettings.json, OR");
        Console.WriteLine("  - Run: entra-lab config set --tenant <tenant-id> --client <client-id>");
    }
}
