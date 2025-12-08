using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using TokenInspector;

namespace WebAuthzDemo.Services;

public class TokenService : ITokenService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITokenAcquisition _tokenAcquisition;

    public TokenService(IHttpContextAccessor httpContextAccessor, ITokenAcquisition tokenAcquisition)
    {
        _httpContextAccessor = httpContextAccessor;
        _tokenAcquisition = tokenAcquisition;
    }

    public async Task<string?> GetIdTokenAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        // Get the ID token from the authentication properties
        var idToken = await httpContext.GetTokenAsync(OpenIdConnectParameterNames.IdToken);
        return idToken;
    }

    public async Task<string?> GetAccessTokenAsync(string[] scopes)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            // Acquire access token for the specified scopes
            var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);
            return accessToken;
        }
        catch (MicrosoftIdentityWebChallengeUserException)
        {
            // This exception is thrown when consent is required
            throw;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public List<Claim> GetUserClaims()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return new List<Claim>();
        }

        return httpContext.User.Claims.ToList();
    }

    public async Task<JwtTokenParts?> GetIdTokenPartsAsync()
    {
        var idToken = await GetIdTokenAsync();
        if (string.IsNullOrWhiteSpace(idToken))
        {
            return null;
        }

        try
        {
            return JwtTools.DecodeToken(idToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<JwtTokenParts?> GetAccessTokenPartsAsync(string[] scopes)
    {
        var accessToken = await GetAccessTokenAsync(scopes);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return null;
        }

        try
        {
            return JwtTools.DecodeToken(accessToken);
        }
        catch
        {
            return null;
        }
    }

    public async Task<TimeSpan> GetAccessTokenLifetimeAsync(string[] scopes)
    {
        var accessToken = await GetAccessTokenAsync(scopes);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return TimeSpan.Zero;
        }

        try
        {
            return JwtTools.GetTimeToExpiration(accessToken);
        }
        catch
        {
            return TimeSpan.Zero;
        }
    }
}
