using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieAppProject.Models
{
    public class Movie
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [StringLength(100)]
        public string Genre { get; set; }

        [StringLength(100)]
        public string Director { get; set; }

        public float Rating { get; set; }

        [StringLength(255)]
        public string ImageUrl { get; set; }

        [StringLength(255)]
        public string VideoUrl { get; set; }

        public int DurationMinutes { get; set; }

        // Navi Properties
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<MovieActor>? MovieActors { get; set; }
        public ICollection<Wishlist>?WishlistItems { get; set; }
        public ICollection<Cart>?CartItems { get; set; }
        public ICollection<PurchaseItem>? Purchases { get; set; }
        
    }
}
