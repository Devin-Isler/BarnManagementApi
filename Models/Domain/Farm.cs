// Farm Domain Model - Represents a farm in the management system
// Contains all properties and relationships for farm entities

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarnManagementApi.Models.Domain
{
    public class Farm
    {
        // Primary key
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required, MaxLength(500)]
        public string Location { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdatedAt { get; set; }

        // Foreign Key → User (who owns this farm)
        [Required]
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;

        // Navigation property - all animals in this farm
        public virtual ICollection<Animal> Animals { get; set; } = new List<Animal>();
    }
}