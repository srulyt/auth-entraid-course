using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TokenInspector;
using WebAuthzDemo.Services;

namespace WebAuthzDemo.Pages.Tokens;

[Authorize]
public class IdTokenModel : PageModel
{
    private readonly ITokenService _tokenService;

    public IdTokenModel(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public JwtTokenParts? TokenParts { get; set; }
    public string JwtMsUrl { get; set; } = string.Empty;
    public List<KeyClaim> KeyClaims { get; set; } = new();

    public async Task OnGetAsync()
    {
        TokenParts = await _tokenService.GetIdTokenPartsAsync();

        if (TokenParts != null)
        {
            JwtMsUrl = JwtTools.GetJwtMsUrl(TokenParts.RawToken);
            ExtractKeyClaims();
        }
    }

    private void ExtractKeyClaims()
    {
        if (TokenParts == null) return;

        var claimDescriptions = new Dictionary<string, string>
        {
            { "iss", "Issuer - The identity provider that issued the token" },
            { "aud", "Audience - The intended recipient of the token (your app's Client ID)" },
            { "sub", "Subject - Unique identifier for the user in this tenant" },
            { "tid", "Tenant ID - The Azure AD tenant that the user belongs to" },
            { "oid", "Object ID - Unique identifier for the user across tenants" },
            { "preferred_username", "Preferred Username - The user's preferred username (usually email)" },
            { "name", "Name - The user's display name" },
            { "email", "Email - The user's email address" },
            { "iat", "Issued At - When the token was issued (Unix timestamp)" },
            { "exp", "Expiration - When the token expires (Unix timestamp)" },
            { "nbf", "Not Before - Token is not valid before this time" },
            { "nonce", "Nonce - Random value to prevent replay attacks" },
            { "ver", "Version - Token version" }
        };

        KeyClaims = TokenParts.Claims
            .Where(c => claimDescriptions.ContainsKey(c.Type))
            .Select(c => new KeyClaim
            {
                Type = c.Type,
                Value = c.Value,
                Description = claimDescriptions[c.Type]
            })
            .ToList();
    }

    public class KeyClaim
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
