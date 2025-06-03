using System.ComponentModel.DataAnnotations;

namespace TruckFreight.WebAPI.Models.Dtos
{
    public class DriverDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string VehicleNumber { get; set; }

        public string VehicleType { get; set; }
        public string VehicleModel { get; set; }
        public string VehicleColor { get; set; }
        public string VehicleRegistrationNumber { get; set; }
        public string VehicleInspectionCertificate { get; set; }

        public bool IsVerified { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string ProfilePhotoUrl { get; set; }

        // Additional information
        public string NationalId { get; set; }
        public string Address { get; set; }
        public string Notes { get; set; }

        // Current location
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }
        public string CurrentLocation { get; set; }
        public DateTime? LastLocationUpdate { get; set; }

        // Statistics
        public int TotalDeliveries { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public double Rating { get; set; }

        // Navigation properties
        public List<CargoRequestDto> RecentCargoRequests { get; set; }
    }
} 