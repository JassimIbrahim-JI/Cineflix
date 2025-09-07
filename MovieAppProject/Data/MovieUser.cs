using Microsoft.AspNetCore.Identity;
using MovieAppProject.Models;
using System.ComponentModel.DataAnnotations;

namespace MovieAppProject.Data
{
    public class MovieUser : IdentityUser
    {
        [PersonalData]
        [StringLength(100)]
        public string FirstName { get; set; }

        [PersonalData]
        public string LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        [PersonalData]
        public DateTime DateOfBirth { get; set; }

        [PersonalData]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // المسختدم بيقدر يكتب اكثر من تعليق وبقدر يضيف اكثر من عنصر داخل السلة وبيشتري اكثر من فلم
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Wishlist> WishlistItems { get; set; }
        public ICollection<Cart>CartItems { get; set; }
        public ICollection<Purchase> Purchases { get; set; }
    }
}
