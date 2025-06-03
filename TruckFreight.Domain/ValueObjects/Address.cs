using System.ComponentModel.DataAnnotations;

namespace TruckFreight.Domain.ValueObjects
{
    public class Address : IEquatable<Address>
    {
        public string Street { get; private set; }
        public string City { get; private set; }
        public string Province { get; private set; }
        public string PostalCode { get; private set; }
        public string Country { get; private set; }
        public double Latitude { get; private set; }
        public double Longitude { get; private set; }

        protected Address() { }

        public Address(string street, string city, string province, string postalCode, 
                      string country, double latitude, double longitude)
        {
            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City cannot be empty", nameof(city));
            
            if (string.IsNullOrWhiteSpace(province))
                throw new ArgumentException("Province cannot be empty", nameof(province));

            if (latitude < -90 || latitude > 90)
                throw new ArgumentException("Latitude must be between -90 and 90", nameof(latitude));

            if (longitude < -180 || longitude > 180)
                throw new ArgumentException("Longitude must be between -180 and 180", nameof(longitude));

            Street = street ?? string.Empty;
            City = city;
            Province = province;
            PostalCode = postalCode ?? string.Empty;
            Country = country ?? "Iran";
            Latitude = latitude;
            Longitude = longitude;
        }

        public string GetFullAddress()
        {
            var parts = new[] { Street, City, Province, PostalCode, Country }
                       .Where(x => !string.IsNullOrWhiteSpace(x));
            return string.Join(", ", parts);
        }

        public double CalculateDistanceTo(Address other)
        {
            // Haversine formula for calculating distance between two points
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

        public bool Equals(Address other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return Street == other.Street &&
                   City == other.City &&
                   Province == other.Province &&
                   PostalCode == other.PostalCode &&
                   Country == other.Country &&
                   Math.Abs(Latitude - other.Latitude) < 0.0001 &&
                   Math.Abs(Longitude - other.Longitude) < 0.0001;
        }

        public override bool Equals(object obj) => Equals(obj as Address);

        public override int GetHashCode()
        {
            return HashCode.Combine(Street, City, Province, PostalCode, Country, Latitude, Longitude);
        }

        public static bool operator ==(Address left, Address right) => 
            ReferenceEquals(left, right) || (left?.Equals(right) ?? false);

        public static bool operator !=(Address left, Address right) => !(left == right);
    }
}
