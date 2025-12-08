using System.Collections.Concurrent;

namespace WebAuthzDemo.Services;

/// <summary>
/// In-memory role store for demonstration purposes.
/// In production, this would be backed by a database.
/// </summary>
public class LocalRoleService : ILocalRoleService
{
    // Thread-safe dictionary: UserId -> List of Roles
    private static readonly ConcurrentDictionary<string, HashSet<string>> _userRoles = new();

    public bool HasRole(string userId, string roleName)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleName))
            return false;

        return _userRoles.TryGetValue(userId, out var roles) && 
               roles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }

    public void AssignRole(string userId, string roleName)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleName))
            return;

        _userRoles.AddOrUpdate(
            userId,
            _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase) { roleName },
            (_, roles) =>
            {
                roles.Add(roleName);
                return roles;
            });
    }

    public void RemoveRole(string userId, string roleName)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(roleName))
            return;

        if (_userRoles.TryGetValue(userId, out var roles))
        {
            roles.Remove(roleName);
            
            // Clean up if no roles left
            if (roles.Count == 0)
            {
                _userRoles.TryRemove(userId, out _);
            }
        }
    }

    public IEnumerable<string> GetUserRoles(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return Enumerable.Empty<string>();

        return _userRoles.TryGetValue(userId, out var roles) 
            ? roles.ToList() 
            : Enumerable.Empty<string>();
    }
}
