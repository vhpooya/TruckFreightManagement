using System;
using System.Collections.Generic;

namespace TruckFreight.Application.Features.Users.DTOs
{
    public class UpdateProfileDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string NationalId { get; set; }
        public string Address { get; set; }
        public string ProfilePicture { get; set; }
        public Dictionary<string, string> AdditionalInfo { get; set; }
    }

    public class UserProfileDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string NationalId { get; set; }
        public string Address { get; set; }
        public string ProfilePicture { get; set; }
        public string UserType { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public Dictionary<string, string> AdditionalInfo { get; set; }
        public CompanyProfileDto Company { get; set; }
        public DriverProfileDto Driver { get; set; }
    }

    public class CompanyProfileDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string RegistrationNumber { get; set; }
        public string EconomicCode { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Logo { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<VehicleDto> Vehicles { get; set; }
        public List<DriverDto> Drivers { get; set; }
    }

    public class DriverProfileDto
    {
        public string Id { get; set; }
        public string LicenseNumber { get; set; }
        public string LicenseType { get; set; }
        public DateTime LicenseExpiryDate { get; set; }
        public string LicensePicture { get; set; }
        public string NationalIdPicture { get; set; }
        public string Status { get; set; }
        public decimal Rating { get; set; }
        public int TotalDeliveries { get; set; }
        public int CompletedDeliveries { get; set; }
        public int CanceledDeliveries { get; set; }
        public decimal OnTimeDeliveryRate { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<VehicleDto> AssignedVehicles { get; set; }
    }

    public class VehicleDto
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public string RegistrationNumber { get; set; }
        public string RegistrationCardPicture { get; set; }
        public string InspectionCertificatePicture { get; set; }
        public string Status { get; set; }
        public bool MaintenanceRequired { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public decimal TotalDistance { get; set; }
        public decimal AverageFuelConsumption { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DriverDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string LicenseNumber { get; set; }
        public string Status { get; set; }
        public decimal Rating { get; set; }
        public int TotalDeliveries { get; set; }
        public decimal OnTimeDeliveryRate { get; set; }
    }
} 