namespace TruckFreight.Domain.ValueObjects
{
    public class GeoLocation : IEquatable<GeoLocation>
    {
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public DateTime Timestamp { get; private set; }
        public double? Accuracy { get; private set; }  // meters
        public double? Speed { get; private set; }     // km/h
        public double? Heading { get; private set; }   // degrees

        protected GeoLocation() { }

        public GeoLocation(double latitude, double longitude, DateTime? timestamp = null, 
                          double? accuracy = null, double? speed = null, double? heading = null)
        {
            if (latitude < -90 || latitude > 90)
                throw new ArgumentException("Latitude must be between -90 and 90", nameof(latitude));

            if (longitude < -180 || longitude > 180)
                throw new ArgumentException("Longitude must be between -180 and 180", nameof(longitude));

            if (accuracy.HasValue && accuracy < 0)
                throw new ArgumentException("Accuracy cannot be negative", nameof(accuracy));

            if (speed.HasValue && speed < 0)
                throw new ArgumentException("Speed cannot be negative", nameof(speed));

            if (heading.HasValue && (heading < 0 || heading >= 360))
                throw new ArgumentException("Heading must be between 0 and 359", nameof(heading));

            Latitude = latitude;
            Longitude = longitude;
            Timestamp = timestamp ?? DateTime.UtcNow;
            Accuracy = accuracy;
            Speed = speed;
            Heading = heading;
        }

        public double CalculateDistanceTo(GeoLocation other)
        {
            const double R = 6371; // Earth's radius in kilometers

            var dLat = ToRadians(other.Latitude - Latitude);
            var dLon = ToRadians(other.Longitude - Longitude);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180;

        public bool IsWithinRadius(GeoLocation center, double radiusKm)
        {
            return CalculateDistanceTo(center) <= radiusKm;
        }

        public string GetCoordinatesString()
        {
            return $"{Latitude:F6}, {Longitude:F6}";
        }

        public bool Equals(GeoLocation other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return Math.Abs(Latitude - other.Latitude) < 0.0001 &&
                   Math.Abs(Longitude - other.Longitude) < 0.0001;
        }

        public override bool Equals(object obj) => Equals(obj as GeoLocation);

        public override int GetHashCode() => HashCode.Combine(Latitude, Longitude);

        public static bool operator ==(GeoLocation left, GeoLocation right) => 
            ReferenceEquals(left, right) || (left?.Equals(right) ?? false);

        public static bool operator !=(GeoLocation left, GeoLocation right) => !(left == right);
    }
}
