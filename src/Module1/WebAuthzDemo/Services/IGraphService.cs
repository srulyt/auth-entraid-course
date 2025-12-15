namespace WebAuthzDemo.Services;

public interface IGraphService
{
    Task<GraphUserProfile?> GetMyProfileAsync();
    Task<bool> TestGraphAccessAsync();
}

public class GraphUserProfile
{
    public string? DisplayName { get; set; }
    public string? Mail { get; set; }
    public string? UserPrincipalName { get; set; }
    public string? Id { get; set; }
    public string? JobTitle { get; set; }
}
