using System;
using System.Collections.Generic;

namespace TruckFreight.Application.Features.Tracking.DTOs
{
    public class UpdateLocationDto
    {
        public Guid DeliveryId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Speed { get; set; }
        public double? Heading { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class LocationDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Speed { get; set; }
        public double? Heading { get; set; }
        public DateTime Timestamp { get; set; }
        public string Address { get; set; }
    }

    public class RoutePointDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address { get; set; }
        public DateTime? EstimatedArrivalTime { get; set; }
        public double? DistanceFromPrevious { get; set; }
        public double? DurationFromPrevious { get; set; }
    }

    public class RouteOptimizationDto
    {
        public List<RoutePointDto> Points { get; set; }
        public double TotalDistance { get; set; }
        public TimeSpan TotalDuration { get; set; }
        public DateTime EstimatedArrivalTime { get; set; }
        public List<string> TrafficAlerts { get; set; }
        public List<string> RoadRestrictions { get; set; }
    }
} 