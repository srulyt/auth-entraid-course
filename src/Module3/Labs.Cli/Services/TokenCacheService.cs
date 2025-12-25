using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace Labs.Cli.Services;

public class TokenCacheService
{
    private const string CacheFileName = "msal_token_cache.bin";
    private const string KeyChainServiceName = "entra_lab_token_cache";
    private const string KeyChainAccountName = "MSALCache";
    private const string LinuxKeyRingSchema = "com.entralab.tokencache";
    private const string LinuxKeyRingCollection = MsalCacheHelper.LinuxKeyRingDefaultCollection;
    private const string LinuxKeyRingLabel = "MSAL token cache for Entra Lab CLI";
    private static readonly string CacheDirectory = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".entra-lab"
    );

    private MsalCacheHelper? _cacheHelper;

    public async Task RegisterCacheAsync(IPublicClientApplication app)
    {
        try
        {
            // Ensure cache directory exists
            if (!Directory.Exists(CacheDirectory))
            {
                Directory.CreateDirectory(CacheDirectory);
            }

            var storageProperties = new StorageCreationPropertiesBuilder(
                CacheFileName,
                CacheDirectory)
                .WithLinuxKeyring(
                    LinuxKeyRingSchema,
                    LinuxKeyRingCollection,
                    LinuxKeyRingLabel,
                    new KeyValuePair<string, string>("Version", "1"),
                    new KeyValuePair<string, string>("Product", "EntraLabCLI"))
                .WithMacKeyChain(
                    KeyChainServiceName,
                    KeyChainAccountName)
                .Build();

            _cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);
            _cacheHelper.RegisterCache(app.UserTokenCache);
        }
        catch (Exception ex)
        {
            // If cache registration fails, the app will still work but without persistence
            Console.WriteLine($"Warning: Could not register token cache: {ex.Message}");
        }
    }

    public void ClearCache()
    {
        try
        {
            var cacheFilePath = Path.Combine(CacheDirectory, CacheFileName);
            if (File.Exists(cacheFilePath))
            {
                File.Delete(cacheFilePath);
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to clear token cache: {ex.Message}", ex);
        }
    }

    public string GetCacheLocation()
    {
        return Path.Combine(CacheDirectory, CacheFileName);
    }

    public bool CacheExists()
    {
        var cacheFilePath = Path.Combine(CacheDirectory, CacheFileName);
        return File.Exists(cacheFilePath);
    }

    public long GetCacheSize()
    {
        var cacheFilePath = Path.Combine(CacheDirectory, CacheFileName);
        if (File.Exists(cacheFilePath))
        {
            var fileInfo = new FileInfo(cacheFilePath);
            return fileInfo.Length;
        }
        return 0;
    }
}
