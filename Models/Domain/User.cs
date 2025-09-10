using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarnManagementApi.Models.Domain
{
    public class User
    {
        public Guid Id { get; set; }

        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public decimal Balance { get; set; } = 1000; // starting balance

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // 1 User → N Farms
        public virtual ICollection<Farm> Farms { get; set; } = new List<Farm>();
    }
}