using System.ComponentModel.DataAnnotations;

namespace TruckFreight.WebAPI.Models.Dtos
{
    public class CargoRequestDto
    {
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
        [Range(0.1, double.MaxValue)]
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
        [Range(0, double.MaxValue)]
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
        public CargoOwnerDto? CargoOwner { get; set; }
        public DriverDto? Driver { get; set; }
        public ICollection<LocationUpdateDto>? LocationUpdates { get; set; }
    }
} 