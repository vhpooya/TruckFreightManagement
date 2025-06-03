using System;
using System.Collections.Generic;

namespace TruckFreight.Application.Features.CargoOwners.DTOs
{
    public class CargoOwnerDto
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string CompanyName { get; set; }
        public string NationalId { get; set; }
        public string EconomicCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsVerified { get; set; }
        public ICollection<CargoDto> Cargos { get; set; }
    }

    public class CargoDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Weight { get; set; }
        public decimal Volume { get; set; }
        public string CargoType { get; set; }
        public string PickupLocation { get; set; }
        public string DeliveryLocation { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 