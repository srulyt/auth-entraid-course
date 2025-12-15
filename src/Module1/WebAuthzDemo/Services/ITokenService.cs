using System.Security.Claims;
using TokenInspector;

namespace WebAuthzDemo.Services;

public interface ITokenService
{
    Task<string?> GetIdTokenAsync();
    Task<string?> GetAccessTokenAsync(string[] scopes);
    List<Claim> GetUserClaims();
    Task<JwtTokenParts?> GetIdTokenPartsAsync();
    Task<JwtTokenParts?> GetAccessTokenPartsAsync(string[] scopes);
    Task<TimeSpan> GetAccessTokenLifetimeAsync(string[] scopes);
}
