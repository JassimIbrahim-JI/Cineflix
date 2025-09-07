using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieAppProject.Models
{
    public class MovieActor
    {
        [StringLength(100)]
        public string CharacterName { get; set; }

        public int MovieId { get; set; }
        [ForeignKey(nameof(MovieId))]
        public Movie Movie { get; set; }

        public int ActorId { get; set; }

        [ForeignKey(nameof(ActorId))]
        public Actor Actor { get; set; }
    }
}
