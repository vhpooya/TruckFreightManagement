using System;
using System.Collections.Generic;

namespace TruckFreight.Application.Features.Drivers.DTOs
{
    public class UpdateDriverStatusDto
    {
        public Guid DriverId { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
    }

    public class DriverDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NationalId { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string VehicleType { get; set; }
        public string VehiclePlateNumber { get; set; }
        public string VehicleRegistrationNumber { get; set; }
        public string Status { get; set; }
        public double Rating { get; set; }
        public int TotalDeliveries { get; set; }
        public int CompletedDeliveries { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class DriverDetailsDto : DriverDto
    {
        public string VehicleInspectionCertificateNumber { get; set; }
        public DateTime VehicleInspectionExpiryDate { get; set; }
        public string ProfilePhotoUrl { get; set; }
        public string NationalIdPhotoUrl { get; set; }
        public string VehiclePhotoUrl { get; set; }
        public string VehicleRegistrationPhotoUrl { get; set; }
        public string VehicleInspectionPhotoUrl { get; set; }
        public List<DeliveryHistoryDto> RecentDeliveries { get; set; }
    }

    public class DeliveryHistoryDto
    {
        public Guid Id { get; set; }
        public string CargoType { get; set; }
        public string PickupLocation { get; set; }
        public string DeliveryLocation { get; set; }
        public DateTime PickupDateTime { get; set; }
        public DateTime DeliveryDateTime { get; set; }
        public string Status { get; set; }
        public double Rating { get; set; }
    }

    public class AvailableDriverDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string VehicleType { get; set; }
        public string VehiclePlateNumber { get; set; }
        public double Rating { get; set; }
        public int CompletedDeliveries { get; set; }
        public string CurrentLocation { get; set; }
        public double DistanceToPickup { get; set; }
        public TimeSpan EstimatedArrivalTime { get; set; }
    }

    public class AvailableDriverListDto
    {
        public List<AvailableDriverDto> Items { get; set; }
        public int TotalCount { get; set; }
    }
} 