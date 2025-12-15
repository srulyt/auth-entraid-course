using Microsoft.AspNetCore.Authorization;

namespace Labs.MiddleTierApi.Authorization;

/// <summary>
/// Authorization requirement that checks if the user's token contains a specific scope
/// </summary>
public class ScopeRequirement : IAuthorizationRequirement
{
    public string RequiredScope { get; }

    public ScopeRequirement(string requiredScope)
    {
        RequiredScope = requiredScope ?? throw new ArgumentNullException(nameof(requiredScope));
    }
}
