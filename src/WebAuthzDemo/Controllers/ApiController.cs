using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAuthzDemo.Authorization;
using WebAuthzDemo.Services;

namespace WebAuthzDemo.Controllers;

[ApiController]
[Route("api")]
public class ApiController : ControllerBase
{
    private readonly IGraphService _graphService;
    private readonly ILogger<ApiController> _logger;

    public ApiController(IGraphService graphService, ILogger<ApiController> logger)
    {
        _graphService = graphService;
        _logger = logger;
    }

    /// <summary>
    /// Example 1: Authentication-Only Authorization
    /// Learning Goal: Understand the difference between AuthN and AuthZ
    /// Only requires the user to be authenticated (signed in)
    /// </summary>
    [HttpGet("authenticated")]
    [Authorize]
    public IActionResult GetAuthenticatedData()
    {
        var userName = User.Identity?.Name ?? "Unknown";
        var userId = User.FindFirst("oid")?.Value ?? "Unknown";

        return Ok(new
        {
            success = true,
            message = "You are authenticated! This endpoint only requires you to be signed in.",
            user = new
            {
                name = userName,
                id = userId,
                authenticationType = User.Identity?.AuthenticationType
            },
            teachingPoint = "Authentication proves WHO you are. Any authenticated user can access this endpoint."
        });
    }

    /// <summary>
    /// Example 2: Local Admin RBAC
    /// Learning Goal: Understand that your app can maintain its own role assignments
    /// Requires the local "Admin" role
    /// </summary>
    [HttpGet("admin-only")]
    [Authorize(Policy = AuthorizationPolicies.RequireLocalAdmin)]
    public IActionResult GetAdminData([FromServices] ILocalRoleService roleService)
    {
        var userName = User.Identity?.Name ?? "Unknown";
        var userId = User.FindFirst("oid")?.Value 
                   ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value 
                   ?? "Unknown";
        
        var userRoles = roleService.GetUserRoles(userId).ToList();

        return Ok(new
        {
            success = true,
            message = "You have the Admin role in our application's local role store!",
            user = new
            {
                name = userName,
                userId = userId,
                localRoles = userRoles
            },
            teachingPoint = "Your application maintains its own role assignments. Entra ID provides WHO you are (identity), but YOUR app decides WHAT you can do (permissions)."
        });
    }

    /// <summary>
    /// Assign the Admin role to the current user
    /// </summary>
    [HttpPost("roles/assign-admin")]
    [Authorize]
    public IActionResult AssignAdminRole([FromServices] ILocalRoleService roleService)
    {
        var userId = User.FindFirst("oid")?.Value 
                   ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new { error = "Could not identify user" });
        }

        roleService.AssignRole(userId, "Admin");

        return Ok(new
        {
            success = true,
            message = "Admin role assigned successfully!",
            userId = userId,
            roles = roleService.GetUserRoles(userId).ToList()
        });
    }

    /// <summary>
    /// Remove the Admin role from the current user
    /// </summary>
    [HttpPost("roles/remove-admin")]
    [Authorize]
    public IActionResult RemoveAdminRole([FromServices] ILocalRoleService roleService)
    {
        var userId = User.FindFirst("oid")?.Value 
                   ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new { error = "Could not identify user" });
        }

        roleService.RemoveRole(userId, "Admin");

        return Ok(new
        {
            success = true,
            message = "Admin role removed successfully!",
            userId = userId,
            roles = roleService.GetUserRoles(userId).ToList()
        });
    }

    /// <summary>
    /// Example 3: Call Microsoft Graph API
    /// Learning Goal: Understand calling external services that require auth
    /// Calls Microsoft Graph /me endpoint using the User.Read permission
    /// </summary>
    [HttpGet("my-profile")]
    [Authorize]
    public async Task<IActionResult> GetMyProfile()
    {
        try
        {
            var profile = await _graphService.GetMyProfileAsync();

            if (profile == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Profile not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Successfully called Microsoft Graph API on your behalf using the User.Read permission.",
                profile = new
                {
                    displayName = profile.DisplayName,
                    email = profile.Mail ?? profile.UserPrincipalName,
                    id = profile.Id,
                    jobTitle = profile.JobTitle
                },
                teachingPoint = "Your app acquired an access token to call Microsoft Graph on your behalf. This demonstrates calling downstream APIs with delegated permissions."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Microsoft Graph API");
            return StatusCode(500, new
            {
                success = false,
                error = "Failed to call Microsoft Graph API",
                message = ex.Message,
                hint = "Ensure the User.Read permission is configured (it's added by default to new app registrations)."
            });
        }
    }
}
