using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebAuthzDemo.Services;

namespace WebAuthzDemo.Pages;

[Authorize]
public class ClaimsModel : PageModel
{
    private readonly ITokenService _tokenService;

    public ClaimsModel(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public List<Claim> AllClaims { get; set; } = new();
    public Dictionary<string, List<Claim>> ClaimsByCategory { get; set; } = new();

    public void OnGet()
    {
        AllClaims = _tokenService.GetUserClaims()
            .OrderBy(c => c.Type)
            .ToList();

        CategorizeClaims();
    }

    private void CategorizeClaims()
    {
        ClaimsByCategory = new Dictionary<string, List<Claim>>();

        var identityClaims = new[] { "sub", "oid", "tid", "name", "preferred_username", "email", "given_name", "family_name", "upn" };
        var tokenClaims = new[] { "iss", "aud", "iat", "exp", "nbf", "ver", "nonce", "c_hash", "at_hash" };
        var authzClaims = new[] { "scp", "roles", "groups", "wids" };

        ClaimsByCategory["Identity Claims"] = AllClaims
            .Where(c => identityClaims.Contains(c.Type))
            .ToList();

        ClaimsByCategory["Token Claims"] = AllClaims
            .Where(c => tokenClaims.Contains(c.Type))
            .ToList();

        ClaimsByCategory["Authorization Claims"] = AllClaims
            .Where(c => authzClaims.Contains(c.Type) || c.Type == "http://schemas.microsoft.com/identity/claims/scope")
            .ToList();

        var categorizedTypes = identityClaims.Concat(tokenClaims).Concat(authzClaims).ToHashSet();
        var otherClaims = AllClaims
            .Where(c => !categorizedTypes.Contains(c.Type) && c.Type != "http://schemas.microsoft.com/identity/claims/scope")
            .ToList();

        if (otherClaims.Any())
        {
            ClaimsByCategory["Other Claims"] = otherClaims;
        }

        // Remove empty categories
        ClaimsByCategory = ClaimsByCategory
            .Where(kvp => kvp.Value.Any())
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}
