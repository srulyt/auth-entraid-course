using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Labs.ClientWeb.Pages;

[Authorize]
public class IdentityModel : PageModel
{
    public void OnGet()
    {
    }
}
