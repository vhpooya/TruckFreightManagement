using System;
using System.Collections.Generic;
using TruckFreight.Application.Features.CargoOwners.DTOs;

namespace TruckFreight.WebAdmin.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalActiveCargos { get; set; }
        public int TotalActiveDrivers { get; set; }
        public int TotalCargoOwners { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<ActiveCargoViewModel> ActiveCargos { get; set; }
        public List<DriverStatusViewModel> ActiveDrivers { get; set; }
        public List<CargoOwnerDto> RecentCargoOwners { get; set; }
        public List<RevenueChartData> RevenueData { get; set; }
    }

    public class ActiveCargoViewModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string CargoOwnerName { get; set; }
        public string DriverName { get; set; }
        public string VehiclePlate { get; set; }
        public string Status { get; set; }
        public string CurrentLocation { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal Price { get; set; }
    }

    public class DriverStatusViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string VehiclePlate { get; set; }
        public string Status { get; set; }
        public string CurrentLocation { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime LastUpdateTime { get; set; }
    }

    public class RevenueChartData
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }
} 