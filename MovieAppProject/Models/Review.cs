using MovieAppProject.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieAppProject.Models
{
    public class Review
    {
        public int Id { get; set; }
       
        [Required]
        [StringLength(1000)]
        public string Content { get; set; }

        [Range(1,5)]
        public int Rating { get; set; }

        public DateTime ReviewDate { get; set; }


        // One move has multiple reviews 
         // One-to-Many
        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        public string UserId { get; set; } // Assuming I'm using Identity for users
        
        [ForeignKey(nameof(UserId))]
        public MovieUser User { get; set; }
    }
}
