using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Labs.Shared.Models;
using System.Security.Claims;
using Microsoft.Kiota.Abstractions.Authentication;
using Constants = Labs.Shared.Constants;

namespace Labs.MiddleTierApi.Controllers;

// Simple token provider for Graph SDK
internal class TokenProvider : IAccessTokenProvider
{
    private readonly string _token;
    
    public TokenProvider(string token)
    {
        _token = token;
    }
    
    public Task<string> GetAuthorizationTokenAsync(Uri uri, Dictionary<string, object>? additionalAuthenticationContext = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_token);
    }
    
    public AllowedHostsValidator AllowedHostsValidator => new AllowedHostsValidator();
}

[ApiController]
[Route("api")]
[Authorize]
public class SecureController : ControllerBase
{
    private readonly ILogger<SecureController> _logger;
    private readonly ITokenAcquisition _tokenAcquisition;

    public SecureController(
        ILogger<SecureController> logger,
        ITokenAcquisition tokenAcquisition)
    {
        _logger = logger;
        _tokenAcquisition = tokenAcquisition;
    }

    /// <summary>
    /// Returns information about the access token used to call this endpoint
    /// Demonstrates token inspection and claims extraction
    /// </summary>
    [HttpGet("whoami")]
    public IActionResult WhoAmI()
    {
        try
        {
            var claims = User.Claims.ToList();
            
            var response = new TokenClaimsResponse
            {
                TokenType = "Access Token",
                Audience = User.FindFirstValue(Constants.ClaimTypes.Audience) ?? "N/A",
                Issuer = User.FindFirstValue(Constants.ClaimTypes.Issuer) ?? "N/A",
                Scope = User.FindFirstValue(Constants.ClaimTypes.Scope),
                Roles = User.FindAll(Constants.ClaimTypes.Roles).Select(c => c.Value).ToArray(),
                ObjectId = User.FindFirstValue(Constants.ClaimTypes.ObjectId),
                TenantId = User.FindFirstValue(Constants.ClaimTypes.TenantId),
                PreferredUserName = User.FindFirstValue(Constants.ClaimTypes.PreferredUserName),
                AllClaims = claims
                    .GroupBy(c => c.Type)
                    .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(c => c.Value)))
            };

            return Ok(new ApiResponse<TokenClaimsResponse>
            {
                Success = true,
                Message = "Token claims extracted successfully",
                Data = response,
                Metadata = new Dictionary<string, string>
                {
                    ["ClaimCount"] = claims.Count.ToString(),
                    ["AuthenticationType"] = User.Identity?.AuthenticationType ?? "Unknown"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting token claims");
            return StatusCode(500, new ApiResponse<TokenClaimsResponse>
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Scope-protected endpoint requiring the api.read scope
    /// Demonstrates scope-based authorization
    /// </summary>
    [HttpGet("secure")]
    [Authorize(Policy = Constants.Policies.RequireApiReadScope)]
    public IActionResult Secure()
    {
        var userName = User.FindFirstValue(Constants.ClaimTypes.PreferredUserName) 
                       ?? User.FindFirstValue(Constants.ClaimTypes.Name) 
                       ?? "Unknown User";

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = $"Hello, {userName}! You successfully called the scope-protected API endpoint.",
            Data = new
            {
                Endpoint = "/api/secure",
                RequiredScope = Constants.Scopes.ApiRead,
                ActualScope = User.FindFirstValue(Constants.ClaimTypes.Scope),
                Timestamp = DateTime.UtcNow
            }
        });
    }

    /// <summary>
    /// Calls Microsoft Graph /me endpoint on behalf of the user
    /// Demonstrates On-Behalf-Of (OBO) flow for downstream API calls
    /// </summary>
    [HttpGet("graphme")]
    [Authorize(Policy = Constants.Policies.RequireApiReadScope)]
    public async Task<IActionResult> GraphMe()
    {
        try
        {
            // Acquire token for Microsoft Graph using OBO
            // Request User.Read scope explicitly for the OBO flow
            var token = await _tokenAcquisition.GetAccessTokenForUserAsync(new[] { "User.Read" });
            
            // Create GraphServiceClient with the token
            var graphClient = new GraphServiceClient(new BaseBearerTokenAuthenticationProvider(new TokenProvider(token)));
            
            // Call Microsoft Graph /me endpoint using OBO
            var user = await graphClient.Me
                .GetAsync(config =>
                {
                    config.QueryParameters.Select = new[] { "id", "displayName", "userPrincipalName", "mail", "jobTitle" };
                });

            if (user == null)
            {
                return NotFound(new ApiResponse<UserProfile>
                {
                    Success = false,
                    Message = "User profile not found in Microsoft Graph"
                });
            }

            var profile = new UserProfile
            {
                Id = user.Id ?? string.Empty,
                DisplayName = user.DisplayName ?? string.Empty,
                UserPrincipalName = user.UserPrincipalName ?? string.Empty,
                Mail = user.Mail,
                JobTitle = user.JobTitle
            };

            return Ok(new ApiResponse<UserProfile>
            {
                Success = true,
                Message = "Successfully retrieved user profile from Microsoft Graph using On-Behalf-Of flow",
                Data = profile,
                Metadata = new Dictionary<string, string>
                {
                    ["GraphEndpoint"] = "/me",
                    ["Flow"] = "On-Behalf-Of (OBO)",
                    ["OriginalCaller"] = User.FindFirstValue(Constants.ClaimTypes.PreferredUserName) ?? "Unknown"
                }
            });
        }
        catch (ServiceException ex)
        {
            _logger.LogError(ex, "Microsoft Graph API error");
            return StatusCode((int)ex.ResponseStatusCode,
                new ApiResponse<UserProfile>
                {
                    Success = false,
                    Message = $"Graph API Error: {ex.Message}"
                });
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            // This exception occurs when additional user consent is required
            _logger.LogWarning(ex, "User consent required for Microsoft Graph access");
            return StatusCode(403, new ApiResponse<UserProfile>
            {
                Success = false,
                Message = "Additional consent required. The API needs permission to call Microsoft Graph on your behalf. " +
                         "Please ensure the API app registration has 'User.Read' delegated permission to Microsoft Graph, " +
                         "and that admin consent has been granted or the user has consented."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Microsoft Graph");
            return StatusCode(500, new ApiResponse<UserProfile>
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            });
        }
    }
}
