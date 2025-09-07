using System.ComponentModel.DataAnnotations;

namespace MovieAppProject.Models
{
    public class Actor
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Bio { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [StringLength(255)]
        public string ImageUrl { get; set; }


        // One Move has multiple Actors
        // One Actor has multiple Movies
        // Many-to-many
        public ICollection<MovieActor>? MovieActors { get; set; }
    }
}
