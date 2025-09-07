using System.ComponentModel.DataAnnotations;

namespace MovieAppProject.Models.ViewModels.Movies
{
    public class MovieActorInputModel
    {
        public int ActorId { get; set; }

        [Required(ErrorMessage = "Character name is required")]
        [StringLength(100)]
        public string CharacterName { get; set; }
    }
}
