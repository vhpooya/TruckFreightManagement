using System;
using System.Collections.Generic;

namespace TruckFreight.Application.Features.CargoRequests.DTOs
{
    public class CreateCargoRequestDto
    {
        public string CargoType { get; set; }
        public decimal Weight { get; set; }
        public string PickupLocation { get; set; }
        public string DeliveryLocation { get; set; }
        public DateTime PickupDateTime { get; set; }
        public DateTime DeliveryDateTime { get; set; }
        public string DeliveryContactName { get; set; }
        public string DeliveryContactPhone { get; set; }
        public string SpecialInstructions { get; set; }
        public decimal Price { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class UpdateCargoRequestDto
    {
        public Guid Id { get; set; }
        public string CargoType { get; set; }
        public decimal Weight { get; set; }
        public string PickupLocation { get; set; }
        public string DeliveryLocation { get; set; }
        public DateTime PickupDateTime { get; set; }
        public DateTime DeliveryDateTime { get; set; }
        public string DeliveryContactName { get; set; }
        public string DeliveryContactPhone { get; set; }
        public string SpecialInstructions { get; set; }
        public decimal Price { get; set; }
        public string PaymentMethod { get; set; }
    }

    public class CargoRequestDto
    {
        public Guid Id { get; set; }
        public string CargoType { get; set; }
        public decimal Weight { get; set; }
        public string PickupLocation { get; set; }
        public string DeliveryLocation { get; set; }
        public DateTime PickupDateTime { get; set; }
        public DateTime DeliveryDateTime { get; set; }
        public string DeliveryContactName { get; set; }
        public string DeliveryContactPhone { get; set; }
        public string SpecialInstructions { get; set; }
        public decimal Price { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public string CargoOwnerName { get; set; }
        public string DriverName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CargoRequestListDto
    {
        public List<CargoRequestDto> Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
} 