using Microsoft.Graph;
using Microsoft.Identity.Web;

namespace WebAuthzDemo.Services;

public class GraphService : IGraphService
{
    private readonly GraphServiceClient _graphClient;
    private readonly ILogger<GraphService> _logger;

    public GraphService(GraphServiceClient graphClient, ILogger<GraphService> logger)
    {
        _graphClient = graphClient;
        _logger = logger;
    }

    public async Task<GraphUserProfile?> GetMyProfileAsync()
    {
        try
        {
            var user = await _graphClient.Me.Request().GetAsync();

            if (user == null)
            {
                return null;
            }

            return new GraphUserProfile
            {
                DisplayName = user.DisplayName,
                Mail = user.Mail,
                UserPrincipalName = user.UserPrincipalName,
                Id = user.Id,
                JobTitle = user.JobTitle
            };
        }
        catch (ServiceException ex) when (ex.Message.Contains("Insufficient privileges"))
        {
            _logger.LogWarning("Insufficient privileges to call Microsoft Graph: {Message}", ex.Message);
            throw new UnauthorizedAccessException("You need to grant 'User.Read' permission and consent to access Microsoft Graph.", ex);
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            _logger.LogWarning("User consent required: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Microsoft Graph API");
            throw;
        }
    }

    public async Task<bool> TestGraphAccessAsync()
    {
        try
        {
            var user = await _graphClient.Me.Request().GetAsync();
            return user != null;
        }
        catch
        {
            return false;
        }
    }
}
