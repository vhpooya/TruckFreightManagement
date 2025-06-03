using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TruckFreight.Domain.Entities
{
    public enum TrackingEventType
    {
        RequestCreated = 1,
        DriverAssigned = 2,
        DriverEnRoute = 3,
        ArrivedAtPickup = 4,
        LoadingStarted = 5,
        LoadingCompleted = 6,
        InTransit = 7,
        ArrivedAtDestination = 8,
        UnloadingStarted = 9,
        UnloadingCompleted = 10,
        Delivered = 11,
        Cancelled = 12,
        Emergency = 13,
        Delayed = 14,
        WeatherAlert = 15,
        RouteDeviation = 16
    }

    public class CargoTracking : BaseEntity
    {
        //[Key]
        //public int TrackingId { get; set; }

        [Required]
        public int CargoRequestId { get; set; }

        [Required]
        public TrackingEventType EventType { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Speed { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Heading { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? Altitude { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Accuracy { get; set; }

        [StringLength(100)]
        public string DeviceId { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? DistanceCovered { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? RemainingDistance { get; set; }

        public TimeSpan? EstimatedTimeOfArrival { get; set; }

        [StringLength(255)]
        public string PhotoPath { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        public bool IsManualEntry { get; set; }

        public int? CreatedByUserId { get; set; }

        // Weather data at the time/location
        [StringLength(100)]
        public string WeatherCondition { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Temperature { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Humidity { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? WindSpeed { get; set; }

        [StringLength(50)]
        public string Visibility { get; set; }

        // Navigation properties
        [ForeignKey("CargoRequestId")]
        public virtual CargoRequest CargoRequest { get; set; }

        [ForeignKey("CreatedByUserId")]
        public virtual User CreatedByUser { get; set; }

        public CargoTracking()
        {
            Timestamp = DateTime.UtcNow;
            IsManualEntry = false;
        }
    }
}