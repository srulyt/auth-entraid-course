using Microsoft.AspNetCore.Authorization;

namespace WebAuthzDemo.Authorization;

public static class AuthorizationPolicies
{
    public const string RequireLocalAdmin = "RequireLocalAdmin";

    public static void ConfigurePolicies(AuthorizationOptions options)
    {
        // Policy: Require local admin role
        // This checks our application's own role store, not Entra ID
        options.AddPolicy(RequireLocalAdmin, policy =>
        {
            policy.RequireAssertion(context =>
            {
                // Get the local role service from DI
                var httpContext = context.Resource as Microsoft.AspNetCore.Http.DefaultHttpContext;
                if (httpContext == null) return false;

                var roleService = httpContext.RequestServices.GetService<Services.ILocalRoleService>();
                if (roleService == null) return false;

                // Get user's object ID
                var userId = context.User.FindFirst("oid")?.Value 
                           ?? context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
                
                if (string.IsNullOrEmpty(userId)) return false;

                return roleService.HasRole(userId, "Admin");
            });
        });
    }
}
