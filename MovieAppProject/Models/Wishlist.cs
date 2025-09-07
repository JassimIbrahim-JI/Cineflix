namespace MovieAppProject.Models
{
    public class Wishlist
    {
        public int Id { get; set; }
        
        public DateTime AddedDate { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; }

        public string UserId { get; set; }
    }
}
