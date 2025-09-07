using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MovieAppProject.Data;

namespace MovieAppProject.Pages.Account.Manage
{
    public class ChangePasswordModel : PageModel
    {
        private readonly UserManager<MovieUser> _userManager;
        private readonly SignInManager<MovieUser> _signInManager;

        public ChangePasswordModel(UserManager<MovieUser> userManager,
                                   SignInManager<MovieUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public ChangePasswordInput Input { get; set; }

        public class ChangePasswordInput
        {
            public string CurrentPassword { get; set; }
            public string NewPassword { get; set; }
            public string ConfirmPassword { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, Input.CurrentPassword, Input.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);
                TempData["PasswordError"] = "Failed to change password.";
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["PasswordError"] = "Password changed successfully.";
            return RedirectToPage("./Index");
        }
    }
}
