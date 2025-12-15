using Microsoft.AspNetCore.Authorization;
using Constants = Labs.Shared.Constants;

namespace Labs.MiddleTierApi.Authorization;

/// <summary>
/// Authorization handler that validates if the user's token contains a specific scope
/// Handles the case where the 'scp' claim contains space-separated scope values
/// </summary>
public class ScopeRequirementHandler : AuthorizationHandler<ScopeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ScopeRequirement requirement)
    {
        // Try to get the scope claim - check both v2.0 ('scp') and v1.0 (full URI) formats
        var scopeClaim = context.User.FindFirst(Constants.ClaimTypes.Scope) 
                         ?? context.User.FindFirst(Constants.ClaimTypes.ScopeV1);
        
        if (scopeClaim == null)
        {
            // No scope claim found - requirement not met
            return Task.CompletedTask;
        }

        // Split the scope claim value by space to get individual scopes
        var scopes = scopeClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Check if the required scope is in the list
        if (scopes.Contains(requirement.RequiredScope, StringComparer.OrdinalIgnoreCase))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
