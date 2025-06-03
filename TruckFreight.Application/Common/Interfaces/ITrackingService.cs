using System;
using System.Threading.Tasks;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Common.Interfaces
{
    public interface ITrackingService
    {
        Task<Result<TripTracking>> AddTrackingPointAsync(
            Guid tripId,
            string location,
            double latitude,
            double longitude,
            double? speed = null,
            string speedUnit = "km/h",
            double? fuelLevel = null,
            string fuelUnit = "L",
            string notes = null);

        Task<Result<TripTracking[]>> GetTripTrackingPointsAsync(
            Guid tripId,
            DateTime? startTime = null,
            DateTime? endTime = null);

        Task<Result<TripTracking>> GetLatestTrackingPointAsync(Guid tripId);
        Task<Result<double>> CalculateDistanceAsync(Guid tripId);
        Task<Result<double>> CalculateEstimatedArrivalTimeAsync(Guid tripId);
        Task<Result> ValidateTrackingPointAsync(TripTracking trackingPoint);
    }
} 