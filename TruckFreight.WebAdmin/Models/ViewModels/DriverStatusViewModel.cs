using System;

namespace TruckFreight.WebAdmin.Models.ViewModels
{
    public class DriverStatusViewModel
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string VehicleNumber { get; set; }
        public string VehicleType { get; set; }
        public string CurrentCargoId { get; set; }
        public string CurrentCargoTitle { get; set; }
        public string Status { get; set; }
        public double CurrentLatitude { get; set; }
        public double CurrentLongitude { get; set; }
        public string CurrentLocation { get; set; }
        public DateTime LastLocationUpdate { get; set; }
        public bool IsOnline { get; set; }
        public double Rating { get; set; }
        public int TotalDeliveries { get; set; }
        public int SuccessfulDeliveries { get; set; }
        public TimeSpan AverageDeliveryTime { get; set; }
        public bool IsAvailable { get; set; }
        public string CurrentRoute { get; set; }
        public double CurrentSpeed { get; set; }
        public string CurrentWeather { get; set; }
        public string TrafficCondition { get; set; }
    }
} 