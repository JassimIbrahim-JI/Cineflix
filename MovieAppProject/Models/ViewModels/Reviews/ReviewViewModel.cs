using System.ComponentModel.DataAnnotations;

namespace MovieAppProject.Models.ViewModels.Reviews
{
    public class ReviewViewModel
    {
        public int Id { get; set; }

        [Required]
        public int MovieId { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Review content must be less than 1000 characters.")]
        public string Content { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating Must be between 1 and 5.")]
        public int Rating { get; set; }

        public DateTime ReviewDate { get; set; }

        public string MovieTitle { get; set; }
        public string Username { get; set; }
    }
}
