// User Domain Model - Represents a user in the farm management system
// Contains all properties and relationships for user entities

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarnManagementApi.Models.Domain
{
    public class User
    {
        // Primary key (matches ASP.NET Identity user ID)
        public Guid Id { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public decimal Balance { get; set; } = 0; // starting balance

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation property - all farms owned by this user
        public virtual ICollection<Farm> Farms { get; set; } = new List<Farm>();
    }
}