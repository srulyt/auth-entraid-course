using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace TokenInspector;

public static class JwtTools
{
    /// <summary>
    /// Decodes a JWT token and returns its header, payload, and signature parts.
    /// </summary>
    public static JwtTokenParts DecodeToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be null or empty", nameof(token));
        }

        var handler = new JwtSecurityTokenHandler();
        
        if (!handler.CanReadToken(token))
        {
            throw new ArgumentException("Invalid JWT token format", nameof(token));
        }

        var jwtToken = handler.ReadJwtToken(token);
        
        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            throw new ArgumentException("Invalid JWT token structure", nameof(token));
        }

        var headerJson = DecodeBase64Url(parts[0]);
        var payloadJson = DecodeBase64Url(parts[1]);

        return new JwtTokenParts
        {
            Header = FormatJson(headerJson),
            Payload = FormatJson(payloadJson),
            Signature = parts[2],
            RawToken = token,
            Claims = jwtToken.Claims.ToList(),
            ValidFrom = jwtToken.ValidFrom,
            ValidTo = jwtToken.ValidTo,
            Issuer = jwtToken.Issuer,
            Audiences = jwtToken.Audiences.ToList()
        };
    }

    /// <summary>
    /// Extracts all claims from a JWT token.
    /// </summary>
    public static List<Claim> GetClaims(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return new List<Claim>();
        }

        var handler = new JwtSecurityTokenHandler();
        
        if (!handler.CanReadToken(token))
        {
            return new List<Claim>();
        }

        var jwtToken = handler.ReadJwtToken(token);
        return jwtToken.Claims.ToList();
    }

    /// <summary>
    /// Gets a specific claim value from a JWT token.
    /// </summary>
    public static string? GetClaimValue(string token, string claimType)
    {
        var claims = GetClaims(token);
        return claims.FirstOrDefault(c => c.Type == claimType)?.Value;
    }

    /// <summary>
    /// Checks if a token contains a specific scope.
    /// </summary>
    public static bool HasScope(string token, string scope)
    {
        var scopeClaim = GetClaimValue(token, "scp");
        if (string.IsNullOrWhiteSpace(scopeClaim))
        {
            return false;
        }

        var scopes = scopeClaim.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return scopes.Contains(scope, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if a token contains a specific role.
    /// </summary>
    public static bool HasRole(string token, string role)
    {
        var claims = GetClaims(token);
        return claims.Any(c => c.Type == "roles" && c.Value.Equals(role, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the time remaining until token expiration.
    /// </summary>
    public static TimeSpan GetTimeToExpiration(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        
        if (!handler.CanReadToken(token))
        {
            return TimeSpan.Zero;
        }

        var jwtToken = handler.ReadJwtToken(token);
        var expirationTime = jwtToken.ValidTo;
        
        return expirationTime - DateTime.UtcNow;
    }

    /// <summary>
    /// Checks if a token is expired.
    /// </summary>
    public static bool IsExpired(string token)
    {
        return GetTimeToExpiration(token) <= TimeSpan.Zero;
    }

    /// <summary>
    /// Redacts the signature portion of a JWT for safe display.
    /// </summary>
    public static string RedactSignature(string token)
    {
        var parts = token.Split('.');
        if (parts.Length != 3)
        {
            return token;
        }

        return $"{parts[0]}.{parts[1]}.[SIGNATURE_REDACTED]";
    }

    /// <summary>
    /// Creates a URL for jwt.ms with the token pre-filled.
    /// </summary>
    public static string GetJwtMsUrl(string token)
    {
        return $"https://jwt.ms/#{token}";
    }

    private static string DecodeBase64Url(string base64Url)
    {
        var base64 = base64Url.Replace('-', '+').Replace('_', '/');
        
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        var bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }

    private static string FormatJson(string json)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(jsonDoc, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
        }
        catch
        {
            return json;
        }
    }
}

public class JwtTokenParts
{
    public string Header { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public string RawToken { get; set; } = string.Empty;
    public List<Claim> Claims { get; set; } = new();
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public string Issuer { get; set; } = string.Empty;
    public List<string> Audiences { get; set; } = new();
}
