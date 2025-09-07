using System.ComponentModel.DataAnnotations;

namespace MovieAppProject.Models.ViewModels.Movies
{
    public class MovieActorViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Actor name is required.")]
        [StringLength(100, ErrorMessage = "Name must be less than 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Biography is required.")]
        [StringLength(1000, ErrorMessage = "Bio must be less than 1000 characters.")]
        public string Bio { get; set; }

        [Required(ErrorMessage = "Image URL is required.")]
        [Url(ErrorMessage = "Please enter a valid URL.")]
        public string ImageUrl { get; set; }

        [Required(ErrorMessage = "Birthday is required.")]
        [DataType(DataType.Date)]
        public DateTime Birthday { get; set; }
    }

}
