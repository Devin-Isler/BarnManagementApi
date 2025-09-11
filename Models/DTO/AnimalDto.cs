using System.ComponentModel.DataAnnotations;

namespace BarnManagementApi.Models.DTO
{
    public class AnimalDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Lifetime { get; set; }
        public int ProductionInterval { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellPrice { get; set; }
        public bool IsActive { get; set; }
        public Guid FarmId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? SoldAt { get; set; }
        public DateTime? LastProductionTime { get; set; }
        public DateTime? DeathTime { get; set; }
    }

    public class AnimalAddDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Value must be positive or zero.")]
        public int Lifetime { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Value must be positive or zero.")]
        public int ProductionInterval { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Value must be positive or zero.")]
        public decimal PurchasePrice { get; set; } = 0;
        [Range(0, int.MaxValue, ErrorMessage = "Value must be positive or zero.")]
        public decimal SellPrice { get; set; }
        [Required]
        public Guid FarmId { get; set; }
       
    }

    public class AnimalUpdateDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Value must be positive or zero.")]
        public int? Lifetime { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Value must be positive or zero.")]
        public int? ProductionInterval { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Value must be positive or zero.")]
        public decimal? PurchasePrice { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Value must be positive or zero.")]
        public decimal? SellPrice { get; set; }

        public Guid? FarmId { get; set; }
    }
}
