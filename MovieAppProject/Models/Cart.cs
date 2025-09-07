using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieAppProject.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int MovieId {  get; set; }
        public string UserId { get; set; }
        public DateTime AddedDate { get; set; }
        public int Quantity { get; set; }

        [ForeignKey(nameof(MovieId))]
        public Movie Movie { get; set; }
    }
}
