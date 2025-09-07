using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MovieAppProject.Data;

namespace MovieAppProject.Pages.Account
{
    public class LogoutModel : PageModel
    {

        private readonly SignInManager<MovieUser> _signManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<MovieUser> signManager, ILogger<LogoutModel> logger)
        {
            _signManager = signManager;
            _logger = logger;
        }
        public async Task<IActionResult> OnPost()
        {
            await _signManager.SignOutAsync();
            _logger.LogInformation("User Logged out.");
            return RedirectToPage("/Account/Login");
        }
    }
}
