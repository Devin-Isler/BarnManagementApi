// Animal Domain Model - Represents an animal in the farm management system
// Contains all properties and relationships for animal entities

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarnManagementApi.Models.Domain
{
    public class Animal
    {
        // Primary key
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int Lifetime { get; set; } // in minutes

        [Required]
        public int ProductionInterval { get; set; } // in minutes

        [Required]
        public decimal PurchasePrice { get; set; }

        [Required]
        public decimal SellPrice { get; set; }

        public bool IsActive { get; set; } = true; 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? SoldAt { get; set; }
        public DateTime? LastProductionTime { get; set; }
        public DateTime? DeathTime { get; set; }

        // Foreign Key → Farm (which farm owns this animal)
        [Required]
        public Guid FarmId { get; set; }
        public virtual Farm Farm { get; set; } = null!;

        // Foreign Key → AnimalType (what type of animal this is)
        [Required]
        public Guid AnimalTypeId { get; set; }
        public virtual AnimalType AnimalType { get; set; } = null!;

        // Navigation property - all products this animal has produced
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}