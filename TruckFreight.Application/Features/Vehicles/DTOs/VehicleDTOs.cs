using System;
using System.Collections.Generic;

namespace TruckFreight.Application.Features.Vehicles.DTOs
{
    public class CreateVehicleDto
    {
        public string Number { get; set; }
        public string Type { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public string RegistrationNumber { get; set; }
        public string RegistrationCardPicture { get; set; }
        public string InspectionCertificatePicture { get; set; }
        public string DriverId { get; set; }
        public Dictionary<string, string> AdditionalInfo { get; set; }
    }

    public class UpdateVehicleDto
    {
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
        public string DriverId { get; set; }
        public Dictionary<string, string> AdditionalInfo { get; set; }
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
        public string CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string DriverId { get; set; }
        public string DriverName { get; set; }
        public Dictionary<string, string> AdditionalInfo { get; set; }
    }

    public class VehicleDetailsDto : VehicleDto
    {
        public List<MaintenanceRecordDto> MaintenanceHistory { get; set; }
        public List<DeliveryRecordDto> DeliveryHistory { get; set; }
        public List<ViolationRecordDto> ViolationHistory { get; set; }
    }

    public class MaintenanceRecordDto
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }
        public string Status { get; set; }
        public string PerformedBy { get; set; }
        public string Location { get; set; }
        public Dictionary<string, string> AdditionalInfo { get; set; }
    }

    public class DeliveryRecordDto
    {
        public string Id { get; set; }
        public string ReferenceNumber { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string PickupLocation { get; set; }
        public string DeliveryLocation { get; set; }
        public decimal Distance { get; set; }
        public decimal Weight { get; set; }
        public decimal Amount { get; set; }
        public decimal FuelConsumption { get; set; }
        public string DriverName { get; set; }
    }

    public class ViolationRecordDto
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public decimal FineAmount { get; set; }
        public DateTime ViolationDate { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public string DriverName { get; set; }
    }

    public class VehicleListDto
    {
        public List<VehicleDto> Vehicles { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
} 