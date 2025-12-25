using Microsoft.Identity.Client;
using Labs.Cli.Helpers;

namespace Labs.Cli.Services;

public class AuthService
{
    private IPublicClientApplication? _app;
    private readonly ConfigService _configService;
    private readonly TokenCacheService _tokenCacheService;
    private string? _cachedAccessToken;

    public AuthService(ConfigService configService, TokenCacheService tokenCacheService)
    {
        _configService = configService;
        _tokenCacheService = tokenCacheService;
    }

    public async Task<IPublicClientApplication> GetAppAsync()
    {
        if (_app == null)
        {
            var config = _configService.GetConfig();
            
            _app = PublicClientApplicationBuilder
                .Create(config.ClientId)
                .WithAuthority(AzureCloudInstance.AzurePublic, config.TenantId)
                .WithRedirectUri(config.RedirectUri)
                .Build();

            await _tokenCacheService.RegisterCacheAsync(_app);
        }

        return _app;
    }

    public async Task<AuthenticationResult> LoginInteractiveAsync()
    {
        var app = await GetAppAsync();
        var config = _configService.GetConfig();

        var result = await app.AcquireTokenInteractive(config.GraphScopes)
            .WithPrompt(Prompt.SelectAccount)
            .ExecuteAsync();

        _cachedAccessToken = result.AccessToken;
        return result;
    }

    public async Task<AuthenticationResult> LoginDeviceCodeAsync()
    {
        var app = await GetAppAsync();
        var config = _configService.GetConfig();

        var result = await app.AcquireTokenWithDeviceCode(config.GraphScopes, deviceCodeResult =>
        {
            Console.WriteLine();
            ConsoleOutput.WriteSubHeader("Device Code Authentication");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"1. Open your browser to: {deviceCodeResult.VerificationUrl}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"2. Enter this code: {deviceCodeResult.UserCode}");
            Console.ResetColor();
            Console.WriteLine();
            ConsoleOutput.WriteInfo("Waiting for authentication...");
            Console.WriteLine();
            
            return Task.CompletedTask;
        }).ExecuteAsync();

        _cachedAccessToken = result.AccessToken;
        return result;
    }

    public async Task<AuthenticationResult?> GetTokenSilentlyAsync()
    {
        try
        {
            var app = await GetAppAsync();
            var config = _configService.GetConfig();
            var accounts = await app.GetAccountsAsync();
            var account = accounts.FirstOrDefault();

            if (account == null)
            {
                return null;
            }

            var result = await app.AcquireTokenSilent(config.GraphScopes, account)
                .ExecuteAsync();

            _cachedAccessToken = result.AccessToken;
            return result;
        }
        catch (MsalUiRequiredException)
        {
            return null;
        }
    }

    public async Task<IAccount?> GetCurrentAccountAsync()
    {
        var app = await GetAppAsync();
        var accounts = await app.GetAccountsAsync();
        return accounts.FirstOrDefault();
    }

    public async Task LogoutAsync()
    {
        var app = await GetAppAsync();
        var accounts = await app.GetAccountsAsync();

        foreach (var account in accounts)
        {
            await app.RemoveAsync(account);
        }

        _cachedAccessToken = null;
        _tokenCacheService.ClearCache();
    }

    public string? GetCachedAccessToken()
    {
        return _cachedAccessToken;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        if (!string.IsNullOrEmpty(_cachedAccessToken))
        {
            return _cachedAccessToken;
        }

        var result = await GetTokenSilentlyAsync();
        return result?.AccessToken;
    }
}
