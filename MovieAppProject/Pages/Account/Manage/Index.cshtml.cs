using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MovieAppProject.Data;

namespace MovieAppProject.Pages.Account.Manage
{
    [Authorize]
    public class IndexModel : PageModel
    {

        private readonly UserManager<MovieUser> _userManager;

        public IndexModel(UserManager<MovieUser> userManager)
        {
            _userManager = userManager;
        }

        public MovieUser CurrentUser { get; set; }

        public async Task<IActionResult> OnGet()
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null) 
            {
                return NotFound($"Unable to load user with ID: '{_userManager.GetUserId(User)}'");
            }
            return Page();
        }
    }
}
