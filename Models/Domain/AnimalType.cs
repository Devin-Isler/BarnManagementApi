using System.ComponentModel.DataAnnotations;

namespace BarnManagementApi.Models.Domain
{
    public class AnimalType
    {
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
        public decimal DefaultSellPrice { get; set; }

        [Required, MaxLength(100)]
        public string ProducedProductName { get; set; } = string.Empty;

        [Required]
        public decimal ProducedProductSellPrice { get; set; }

        public virtual ICollection<Animal> Animals { get; set; } = new List<Animal>();
    }
}


