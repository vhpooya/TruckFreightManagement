using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TruckFreight.WebAPI.Models.Entities
{
    public class CargoRequest
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public string PickupLocation { get; set; }

        [Required]
        public string DeliveryLocation { get; set; }

        [Required]
        public DateTime PickupDate { get; set; }

        public DateTime? DeliveryDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        // Cargo details
        public string CargoType { get; set; }
        public decimal Weight { get; set; }
        public string Dimensions { get; set; }
        public string SpecialInstructions { get; set; }

        // Contact information
        public string PickupContactName { get; set; }
        public string PickupContactPhone { get; set; }
        public string DeliveryContactName { get; set; }
        public string DeliveryContactPhone { get; set; }

        // Tracking information
        public string CurrentLocation { get; set; }
        public decimal? DistanceTraveled { get; set; }
        public decimal? RemainingDistance { get; set; }
        public TimeSpan? EstimatedTimeRemaining { get; set; }
        public bool IsDelayed { get; set; }
        public string DelayReason { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string CancellationReason { get; set; }

        // Foreign keys
        public int CargoOwnerId { get; set; }
        public int? DriverId { get; set; }

        // Navigation properties
        public CargoOwner CargoOwner { get; set; }
        public Driver Driver { get; set; }
    }
} 