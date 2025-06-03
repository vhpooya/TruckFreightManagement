using System;

namespace TruckFreight.WebAdmin.Models.ViewModels
{
    public class RevenueChartData
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int CompletedDeliveries { get; set; }
        public decimal AverageDeliveryPrice { get; set; }
        public decimal PlatformCommission { get; set; }
        public decimal DriverEarnings { get; set; }
        public decimal NetRevenue { get; set; }
    }
} 