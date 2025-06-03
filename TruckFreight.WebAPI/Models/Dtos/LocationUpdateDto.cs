using System.ComponentModel.DataAnnotations;

namespace TruckFreight.WebAPI.Models.Dtos
{
    public class LocationUpdateDto
    {
        [Required]
        [Range(-90, 90)]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180)]
        public double Longitude { get; set; }

        public string Location { get; set; }
    }
} 