using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Labs.Shared.Models;
using System.Text.Json;

namespace Labs.ClientWeb.Pages;

[Authorize]
[AuthorizeForScopes(Scopes = new[] { "api://[your-api-client-id]/.default" })]
public class CallApiModel : PageModel
{
    private readonly ILogger<CallApiModel> _logger;
    private readonly IDownstreamApi _downstreamApi;

    public CallApiModel(
        ILogger<CallApiModel> logger,
        IDownstreamApi downstreamApi)
    {
        _logger = logger;
        _downstreamApi = downstreamApi;
    }

    public ApiResponse<object>? ApiResponse { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostWhoAmIAsync()
    {
        try
        {
            var response = await _downstreamApi.GetForUserAsync<ApiResponse<TokenClaimsResponse>>(
                "MiddleTierApi",
                options => options.RelativePath = "api/whoami");

            ApiResponse = new ApiResponse<object>
            {
                Success = response?.Success ?? false,
                Message = response?.Message ?? "No response",
                Data = response?.Data,
                Metadata = response?.Metadata ?? new()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling /api/whoami");
            ApiResponse = new ApiResponse<object>
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }

        return Page();
    }

    public async Task<IActionResult> OnPostSecureAsync()
    {
        try
        {
            var response = await _downstreamApi.GetForUserAsync<ApiResponse<object>>(
                "MiddleTierApi",
                options => options.RelativePath = "api/secure");

            ApiResponse = response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling /api/secure");
            ApiResponse = new ApiResponse<object>
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }

        return Page();
    }

    public async Task<IActionResult> OnPostGraphMeAsync()
    {
        try
        {
            var response = await _downstreamApi.GetForUserAsync<ApiResponse<UserProfile>>(
                "MiddleTierApi",
                options => options.RelativePath = "api/graphme");

            ApiResponse = new ApiResponse<object>
            {
                Success = response?.Success ?? false,
                Message = response?.Message ?? "No response",
                Data = response?.Data,
                Metadata = response?.Metadata ?? new()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling /api/graphme");
            ApiResponse = new ApiResponse<object>
            {
                Success = false,
                Message = $"Error: {ex.Message}"
            };
        }

        return Page();
    }
}
