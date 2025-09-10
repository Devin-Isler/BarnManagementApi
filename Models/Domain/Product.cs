using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BarnManagementApi.Models.Domain
{
    public class Product
    {
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SoldAt { get; set; }

        [NotMapped]
        public bool IsSold => SoldAt != null;

        // Foreign Key → Animal
        [Required]
        public Guid AnimalId { get; set; }
        public virtual Animal Animal { get; set; } = null!;
    }
}