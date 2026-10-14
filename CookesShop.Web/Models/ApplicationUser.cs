using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CookesShop.Web.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Address { get; set; }

        public int Points { get; set; } = 0;

        // Guest, Silver, Gold
        [StringLength(50)]
        public string Tier { get; set; } = "Guest";
    }
}
