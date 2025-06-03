using System;
using System.Collections.Generic;

namespace TruckFreight.Application.Features.Violations.DTOs
{
    public class CreateViolationDto
    {
        public Guid DeliveryId { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public decimal FineAmount { get; set; }
        public string Evidence { get; set; }
        public string Location { get; set; }
        public DateTime ViolationDate { get; set; }
    }

    public class UpdateViolationStatusDto
    {
        public Guid ViolationId { get; set; }
        public string Status { get; set; }
        public string Resolution { get; set; }
        public DateTime? ResolutionDate { get; set; }
    }

    public class ViolationDto
    {
        public Guid Id { get; set; }
        public Guid DeliveryId { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public decimal FineAmount { get; set; }
        public string Evidence { get; set; }
        public string Location { get; set; }
        public DateTime ViolationDate { get; set; }
        public string Status { get; set; }
        public string Resolution { get; set; }
        public DateTime? ResolutionDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string DriverName { get; set; }
        public string CargoOwnerName { get; set; }
    }

    public class ViolationDetailsDto : ViolationDto
    {
        public string DriverId { get; set; }
        public string CargoOwnerId { get; set; }
        public string DriverPhone { get; set; }
        public string CargoOwnerPhone { get; set; }
        public string DriverEmail { get; set; }
        public string CargoOwnerEmail { get; set; }
        public string DeliveryReference { get; set; }
        public string PickupLocation { get; set; }
        public string DeliveryLocation { get; set; }
        public List<ViolationHistoryDto> ViolationHistory { get; set; }
    }

    public class ViolationHistoryDto
    {
        public string Status { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string UpdatedBy { get; set; }
    }

    public class ViolationListDto
    {
        public List<ViolationDto> Violations { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
} 