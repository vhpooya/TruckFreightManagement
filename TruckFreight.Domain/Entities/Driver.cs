using TruckFreight.Domain.Enums;
using TruckFreight.Domain.ValueObjects;

namespace TruckFreight.Domain.Entities
{
    public class Driver : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string LicenseNumber { get; private set; }
        public DateTime LicenseExpiryDate { get; private set; }
        public string LicenseClass { get; private set; }
        public bool IsAvailable { get; private set; }
        public GeoLocation CurrentLocation { get; private set; }
        public DateTime? LastLocationUpdate { get; private set; }
        public double Rating { get; private set; }
        public int TotalTrips { get; private set; }
        public int CompletedTrips { get; private set; }
        public string EmergencyContactName { get; private set; }
        public PhoneNumber EmergencyContactPhone { get; private set; }

        // Navigation Properties
        public virtual User User { get; private set; }
        public virtual ICollection<Vehicle> Vehicles { get; private set; }
        public virtual ICollection<Trip> Trips { get; private set; }
        public virtual ICollection<DriverRating> Ratings { get; private set; }

        protected Driver()
        {
            Vehicles = new HashSet<Vehicle>();
            Trips = new HashSet<Trip>();
            Ratings = new HashSet<DriverRating>();
        }

        public Driver(Guid userId, string licenseNumber, DateTime licenseExpiryDate, string licenseClass)
            : this()
        {
            UserId = userId;
            LicenseNumber = licenseNumber ?? throw new ArgumentNullException(nameof(licenseNumber));
            LicenseExpiryDate = licenseExpiryDate;
            LicenseClass = licenseClass ?? throw new ArgumentNullException(nameof(licenseClass));
            IsAvailable = false;
            Rating = 5.0;
        }

        public void UpdateLicense(string licenseNumber, DateTime expiryDate, string licenseClass)
        {
            LicenseNumber = licenseNumber;
            LicenseExpiryDate = expiryDate;
            LicenseClass = licenseClass;
        }

        public void UpdateLocation(GeoLocation location)
        {
            CurrentLocation = location;
            LastLocationUpdate = DateTime.UtcNow;
        }

        public void SetAvailability(bool isAvailable)
        {
            IsAvailable = isAvailable;
        }

      
       public void UpdateEmergencyContact(string name, PhoneNumber phone)
       {
           EmergencyContactName = name;
           EmergencyContactPhone = phone;
       }

       public void CompleteTrip()
       {
           CompletedTrips++;
           TotalTrips++;
       }

       public void UpdateRating(double newRating)
       {
           if (newRating >= 1 && newRating <= 5)
           {
               Rating = newRating;
           }
       }

       public bool IsLicenseValid => LicenseExpiryDate > DateTime.UtcNow;
       public bool IsLocationFresh => LastLocationUpdate.HasValue && 
                                     (DateTime.UtcNow - LastLocationUpdate.Value).TotalMinutes < 5;
   }
}

