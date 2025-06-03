using System;

namespace TruckFreight.WebAdmin.Models.ViewModels
{
    public class ActiveCargoViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string CargoOwnerName { get; set; }
        public string DriverName { get; set; }
        public string VehicleNumber { get; set; }
        public string PickupLocation { get; set; }
        public string DeliveryLocation { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
        public double CurrentLatitude { get; set; }
        public double CurrentLongitude { get; set; }
        public string CurrentLocation { get; set; }
        public double DistanceTraveled { get; set; }
        public double RemainingDistance { get; set; }
        public TimeSpan EstimatedTimeRemaining { get; set; }
        public bool IsDelayed { get; set; }
        public string DelayReason { get; set; }
    }
} 