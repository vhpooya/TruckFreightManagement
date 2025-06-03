using System.ComponentModel.DataAnnotations;

namespace TruckFreight.WebAPI.Models.Dtos
{
    public class CargoOwnerDto
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
        public string Type { get; set; } // Individual or Company

        public bool IsVerified { get; set; }
        public bool IsBlocked { get; set; }
        public DateTime RegistrationDate { get; set; }
        public string ProfilePhotoUrl { get; set; }

        // Statistics
        public int TotalCargoRequests { get; set; }
        public int CompletedDeliveries { get; set; }
        public decimal TotalSpent { get; set; }
        public double AverageRating { get; set; }

        // Additional information
        public string NationalId { get; set; }
        public string Occupation { get; set; }
        public string Address { get; set; }
        public string Notes { get; set; }

        // Navigation properties
        public List<CargoRequestDto> RecentCargoRequests { get; set; }
    }
} 