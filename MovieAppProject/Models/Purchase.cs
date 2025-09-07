using MovieAppProject.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieAppProject.Models
{
    public class Purchase
    {
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime PurchaseDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set;}

        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public MovieUser User { get; set; }

        public string PaymentIntentId { get; set; }
        public string PaymentStatus { get; set; } = "Pending";

        public ICollection<PurchaseItem>PurchaseItems { get; set; }
    }
}
