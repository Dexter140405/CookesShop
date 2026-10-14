using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookesShop.Web.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SessionId { get; set; } = string.Empty; // To support guest users

        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        [Required]
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product? Product { get; set; }

        public int Quantity { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }
}
