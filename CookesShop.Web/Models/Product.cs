using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookesShop.Web.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category? Category { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Ingredients { get; set; } // Nguyên liệu

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; } = 0;

        public bool IsHot { get; set; } = false;

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
