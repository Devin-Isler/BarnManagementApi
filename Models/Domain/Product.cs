// Product Domain Model - Represents a product produced by animals
// Contains all properties and relationships for product entities

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarnManagementApi.Models.Domain
{
    public class Product
    {
        // Primary key
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SoldAt { get; set; }

        public bool IsSold { get; set; }

        // Foreign Key → Animal (which animal produced this product)
        [Required]
        public Guid AnimalId { get; set; }
        public virtual Animal Animal { get; set; } = null!;
    }
}