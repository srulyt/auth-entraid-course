using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using TokenInspector;
using WebAuthzDemo.Services;

namespace WebAuthzDemo.Pages.Tokens;

[Authorize]
public class AccessTokenModel : PageModel
{
    private readonly ITokenService _tokenService;

    public AccessTokenModel(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public JwtTokenParts? TokenParts { get; set; }
    public string JwtMsUrl { get; set; } = string.Empty;
    public List<string> Scopes { get; set; } = new();
    public Dictionary<string, string> ImportantClaims { get; set; } = new();
    public string TimeToExpiration { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            var scopes = new[] { "User.Read" };
            TokenParts = await _tokenService.GetAccessTokenPartsAsync(scopes);

            if (TokenParts != null)
            {
                JwtMsUrl = JwtTools.GetJwtMsUrl(TokenParts.RawToken);
                ExtractScopes();
                ExtractImportantClaims();
                CalculateTimeToExpiration();
            }
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            ErrorMessage = $"Consent required: {ex.Message}. Please grant the required permissions.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to acquire access token: {ex.Message}";
        }
    }

    private void ExtractScopes()
    {
        if (TokenParts == null) return;

        // Check for scp claim
        var scopeClaim = TokenParts.Claims.FirstOrDefault(c => c.Type == "scp");
        if (scopeClaim != null)
        {
            Scopes = scopeClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            return;
        }

        // Also check alternative claim name
        var altScopeClaim = TokenParts.Claims.FirstOrDefault(c => 
            c.Type == "http://schemas.microsoft.com/identity/claims/scope");
        if (altScopeClaim != null)
        {
            Scopes = altScopeClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }

    private void ExtractImportantClaims()
    {
        if (TokenParts == null) return;

        var interestingClaimTypes = new[] 
        { 
            "aud", "iss", "sub", "appid", "tid", "oid", 
            "scp", "roles", "ver", "iat", "exp" 
        };

        ImportantClaims = TokenParts.Claims
            .Where(c => interestingClaimTypes.Contains(c.Type))
            .GroupBy(c => c.Type)
            .ToDictionary(g => g.Key, g => g.First().Value);
    }

    private void CalculateTimeToExpiration()
    {
        if (TokenParts == null) return;

        var timeRemaining = TokenParts.ValidTo - DateTime.UtcNow;
        
        if (timeRemaining.TotalSeconds < 0)
        {
            TimeToExpiration = "Expired";
        }
        else if (timeRemaining.TotalMinutes < 1)
        {
            TimeToExpiration = $"{(int)timeRemaining.TotalSeconds} seconds";
        }
        else if (timeRemaining.TotalHours < 1)
        {
            TimeToExpiration = $"{(int)timeRemaining.TotalMinutes} minutes";
        }
        else
        {
            TimeToExpiration = $"{timeRemaining.Hours}h {timeRemaining.Minutes}m";
        }
    }
}
