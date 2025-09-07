using System.ComponentModel.DataAnnotations.Schema;

namespace MovieAppProject.Models
{
    public class PurchaseItem
    {
        public int Id { get; set; }

        [Column(TypeName ="decimal(18,2)")]
        public decimal PriceAtPurchase { get; set; }
        public int Quantity { get; set; } = 1;

        public int MovieId { get; set; }

        [ForeignKey(nameof(MovieId))]
        public Movie Movie { get; set; }

        public int PurchaseId { get; set; }
        public Purchase Purchase { get; set; }
    }
}
