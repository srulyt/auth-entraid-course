namespace WebAuthzDemo.Services;

public interface ILocalRoleService
{
    /// <summary>
    /// Check if a user has a specific role
    /// </summary>
    bool HasRole(string userId, string roleName);

    /// <summary>
    /// Assign a role to a user
    /// </summary>
    void AssignRole(string userId, string roleName);

    /// <summary>
    /// Remove a role from a user
    /// </summary>
    void RemoveRole(string userId, string roleName);

    /// <summary>
    /// Get all roles for a user
    /// </summary>
    IEnumerable<string> GetUserRoles(string userId);
}
