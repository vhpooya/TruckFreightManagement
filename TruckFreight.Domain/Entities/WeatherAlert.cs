using TruckFreight.Domain.Enums;
using TruckFreight.Domain.ValueObjects;

namespace TruckFreight.Domain.Entities
{
    public class WeatherAlert : BaseEntity
    {
        public Guid? TripId { get; private set; }
        public Guid? DriverId { get; private set; }
        public GeoLocation Location { get; private set; }
        public WeatherCondition Condition { get; private set; }
        public WeatherSeverity Severity { get; private set; }
        public string Description { get; private set; }
        public DateTime ValidFrom { get; private set; }
        public DateTime ValidTo { get; private set; }
        public bool IsActive { get; private set; }
        public bool IsAcknowledged { get; private set; }
        public DateTime? AcknowledgedAt { get; private set; }
        public double Temperature { get; private set; }
        public double WindSpeed { get; private set; }
        public double Visibility { get; private set; }
        public string RecommendedAction { get; private set; }

        // Navigation Properties
        public virtual Trip Trip { get; private set; }
        public virtual Driver Driver { get; private set; }

        protected WeatherAlert() { }

        public WeatherAlert(GeoLocation location, WeatherCondition condition, 
                           WeatherSeverity severity, string description,
                           DateTime validFrom, DateTime validTo)
        {
            Location = location ?? throw new ArgumentNullException(nameof(location));
            Condition = condition;
            Severity = severity;
            Description = description ?? throw new ArgumentNullException(nameof(description));
            ValidFrom = validFrom;
            ValidTo = validTo;
            IsActive = true;
        }

        public void AssignToTrip(Guid tripId)
        {
            TripId = tripId;
        }

        public void AssignToDriver(Guid driverId)
        {
            DriverId = driverId;
        }

        public void Acknowledge()
        {
            IsAcknowledged = true;
            AcknowledgedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void UpdateWeatherData(double temperature, double windSpeed, double visibility)
        {
            Temperature = temperature;
            WindSpeed = windSpeed;
            Visibility = visibility;
        }

        public void SetRecommendedAction(string action)
        {
            RecommendedAction = action;
        }

        public bool IsCurrentlyValid => DateTime.UtcNow >= ValidFrom && DateTime.UtcNow <= ValidTo;
        public bool RequiresImmedateAction => Severity >= WeatherSeverity.High && IsActive;
    }
}
