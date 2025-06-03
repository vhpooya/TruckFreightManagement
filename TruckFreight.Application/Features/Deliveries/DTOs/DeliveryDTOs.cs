using System;
using System.Collections.Generic;

namespace TruckFreight.Application.Features.Deliveries.DTOs
{
    public class CreateDeliveryDto
    {
        public Guid CargoRequestId { get; set; }
        public Guid DriverId { get; set; }
        public decimal Price { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class UpdateDeliveryStatusDto
    {
        public Guid DeliveryId { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
        public string Location { get; set; }
    }

    public class ConfirmDeliveryDto
    {
        public Guid DeliveryId { get; set; }
        public string ConfirmationCode { get; set; }
        public string Location { get; set; }
        public string Notes { get; set; }
    }

    public class DeliveryDto
    {
        public Guid Id { get; set; }
        public Guid CargoRequestId { get; set; }
        public Guid DriverId { get; set; }
        public string CargoType { get; set; }
        public decimal Weight { get; set; }
        public string PickupLocation { get; set; }
        public string DeliveryLocation { get; set; }
        public DateTime PickupDateTime { get; set; }
        public DateTime DeliveryDateTime { get; set; }
        public string Status { get; set; }
        public decimal Price { get; set; }
        public string PaymentMethod { get; set; }
        public string DriverName { get; set; }
        public string CargoOwnerName { get; set; }
        public double? Rating { get; set; }
        public string RatingComment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PickedUpAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string CancellationReason { get; set; }
    }

    public class DeliveryDetailsDto : DeliveryDto
    {
        public string DriverPhoneNumber { get; set; }
        public string DriverEmail { get; set; }
        public string DriverVehicleType { get; set; }
        public string DriverVehiclePlateNumber { get; set; }
        public string CargoOwnerPhoneNumber { get; set; }
        public string CargoOwnerEmail { get; set; }
        public string DeliveryContactName { get; set; }
        public string DeliveryContactPhone { get; set; }
        public string SpecialInstructions { get; set; }
        public List<DeliveryTrackingDto> TrackingHistory { get; set; }
    }

    public class DeliveryTrackingDto
    {
        public DateTime Timestamp { get; set; }
        public string Status { get; set; }
        public string Location { get; set; }
        public string Notes { get; set; }
    }

    public class DeliveryListDto
    {
        public List<DeliveryDto> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
} 