using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebAuthzDemo.Services;

namespace WebAuthzDemo.Pages;

[Authorize]
public class ProtectedApiModel : PageModel
{
    private readonly ILocalRoleService _roleService;

    public ProtectedApiModel(ILocalRoleService roleService)
    {
        _roleService = roleService;
    }

    public bool HasLocalAdminRole { get; private set; }
    public List<string> LocalRoles { get; private set; } = new();
    public string UserId { get; private set; } = string.Empty;

    public void OnGet()
    {
        // Get user's object ID
        UserId = User.FindFirst("oid")?.Value 
               ?? User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value 
               ?? "Unknown";

        // Check local role assignments
        LocalRoles = _roleService.GetUserRoles(UserId).ToList();
        HasLocalAdminRole = _roleService.HasRole(UserId, "Admin");
    }
}
