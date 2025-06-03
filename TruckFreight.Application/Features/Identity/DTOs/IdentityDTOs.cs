using System;
using System.Collections.Generic;

namespace TruckFreight.Application.Features.Identity.DTOs
{
    public class RegisterDriverDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NationalId { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string VehicleType { get; set; }
        public string VehiclePlateNumber { get; set; }
        public string VehicleRegistrationNumber { get; set; }
        public string VehicleInspectionCertificateNumber { get; set; }
        public DateTime VehicleInspectionExpiryDate { get; set; }
        public string DriverLicenseNumber { get; set; }
        public DateTime DriverLicenseExpiryDate { get; set; }
        public string ProfilePhotoUrl { get; set; }
        public string VehiclePhotoUrl { get; set; }
        public string NationalIdPhotoUrl { get; set; }
        public string VehicleRegistrationPhotoUrl { get; set; }
        public string VehicleInspectionPhotoUrl { get; set; }
        public string DriverLicensePhotoUrl { get; set; }
    }

    public class RegisterCargoOwnerDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NationalId { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Occupation { get; set; }
        public string ProfilePhotoUrl { get; set; }
        public string NationalIdPhotoUrl { get; set; }
    }

    public class RegisterCompanyDto
    {
        public string CompanyName { get; set; }
        public string RegistrationNumber { get; set; }
        public string TaxId { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Website { get; set; }
        public string RepresentativeFirstName { get; set; }
        public string RepresentativeLastName { get; set; }
        public string RepresentativeNationalId { get; set; }
        public string RepresentativePhoneNumber { get; set; }
        public string RepresentativeEmail { get; set; }
        public string RepresentativePassword { get; set; }
        public string CompanyRegistrationPhotoUrl { get; set; }
        public string TaxCertificatePhotoUrl { get; set; }
        public string RepresentativeProfilePhotoUrl { get; set; }
        public string RepresentativeNationalIdPhotoUrl { get; set; }
    }

    public class VerifyIdentityDto
    {
        public string UserId { get; set; }
        public string VerificationCode { get; set; }
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string DocumentPhotoUrl { get; set; }
    }

    public class IdentityResultDto
    {
        public bool Succeeded { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public List<string> Errors { get; set; }
    }
} 