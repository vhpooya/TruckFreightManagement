using System;
using System.Collections.Generic;
using TruckFreight.Domain.Common;
using TruckFreight.Domain.Enums;
using TruckFreight.Domain.ValueObjects;

namespace TruckFreight.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public bool IsPhoneVerified { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime LastLoginAt { get; set; }
        public string ProfilePictureUrl { get; set; }
        public string NationalId { get; set; }
        public string NationalIdCardUrl { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string DeviceToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Navigation Properties
        public virtual ICollection<Vehicle> Vehicles { get; set; }
        public virtual ICollection<Cargo> Cargos { get; set; }
        public virtual ICollection<Trip> Trips { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }
        public virtual ICollection<Notification> Notifications { get; set; }

        public User()
        {
            Vehicles = new HashSet<Vehicle>();
            Cargos = new HashSet<Cargo>();
            Trips = new HashSet<Trip>();
            Payments = new HashSet<Payment>();
            Notifications = new HashSet<Notification>();
        }

        public string GetFullName() => $"{FirstName} {LastName}";

        public void UpdatePersonalInfo(string firstName, string lastName, string email)
        {
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            Email = email;
        }

        public void VerifyEmail()
        {
            IsEmailVerified = true;
        }

        public void VerifyPhone()
        {
            IsPhoneVerified = true;
        }

        public void UpdateStatus(bool status)
        {
            IsActive = status;
        }

        public void RecordLogin()
        {
            LastLoginAt = DateTime.UtcNow;
        }

        public void UpdateProfileImage(string imageUrl)
        {
            ProfilePictureUrl = imageUrl;
        }

        public bool IsVerified => IsEmailVerified && IsPhoneVerified;
       // public bool IsActive => IsActive;
    }
}
