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
        public Guid FarmId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastProductionTime { get; set; }
        public DateTime? DeathTime { get; set; }
    }

    public class AnimalAddDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required]
        public int Lifetime { get; set; }
        [Required]
        public int ProductionInterval { get; set; }
        [Required]
        public decimal PurchasePrice { get; set; }
        [Required]
        public decimal SellPrice { get; set; }
        [Required]
        public Guid FarmId { get; set; }
    }

    public class AnimalUpdateDto
    {
        [MaxLength(100)]
        public string? Name { get; set; }
        public int? Lifetime { get; set; }
        public int? ProductionInterval { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? SellPrice { get; set; }
        public Guid? FarmId { get; set; }
        public DateTime? LastProductionTime { get; set; }
        public DateTime? DeathTime { get; set; }
    }
}


