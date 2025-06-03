using System;
using System.Collections.Generic;

namespace TruckFreight.Domain.Entities
{
    public class Cargo
    {
        public string Id { get; set; }
        public string CargoOwnerId { get; set; }
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
        public string DriverId { get; set; }
        public string VehicleId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation properties
        public CargoOwner CargoOwner { get; set; }
        public Driver Driver { get; set; }
        public Vehicle Vehicle { get; set; }
        public ICollection<CargoRequestRating> Ratings { get; set; }
        //public ICollection<CargoTracking> TrackingHistory { get; set; }
        //public ICollection<CargoDocument> Documents { get; set; }
    }
} 