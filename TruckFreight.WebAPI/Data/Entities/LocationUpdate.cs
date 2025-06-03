using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TruckFreight.WebAPI.Data.Entities
{
    public class LocationUpdate
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CargoRequestId { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        public string? Location { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        // Navigation property
        [ForeignKey("CargoRequestId")]
        public CargoRequest CargoRequest { get; set; }
    }
} 