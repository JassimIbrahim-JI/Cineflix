using System.ComponentModel.DataAnnotations;

namespace MovieAppProject.Models.ViewModels.User
{
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int ReviewCount { get; set; }
        public int PurchaseCount { get; set; }
        public bool IsAdmin { get; set; }
    }
}
