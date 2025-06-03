using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TruckFreight.WebAPI.Data.Entities
{
    public class CargoRequest
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public string CargoType { get; set; }

        [Required]
        public double Weight { get; set; }

        [Required]
        [StringLength(200)]
        public string PickupLocation { get; set; }

        [Required]
        public double PickupLatitude { get; set; }

        [Required]
        public double PickupLongitude { get; set; }

        [Required]
        [StringLength(200)]
        public string DeliveryLocation { get; set; }

        [Required]
        public double DeliveryLatitude { get; set; }

        [Required]
        public double DeliveryLongitude { get; set; }

        [Required]
        public DateTime PickupDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        public int CargoOwnerId { get; set; }

        public int? DriverId { get; set; }

        [Required]
        public string Status { get; set; }

        public string? CancellationReason { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("CargoOwnerId")]
        public CargoOwner CargoOwner { get; set; }

        [ForeignKey("DriverId")]
        public Driver? Driver { get; set; }

        public ICollection<LocationUpdate> LocationUpdates { get; set; }
    }
} 