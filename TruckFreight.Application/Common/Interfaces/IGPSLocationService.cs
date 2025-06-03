using System;
using System.Threading.Tasks;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Common.Interfaces
{
    public interface IGPSLocationService
    {
        Task<TripTracking> TrackLocationAsync(Guid tripId, string location, decimal latitude, decimal longitude);
        Task<TripTracking> GetLastLocationAsync(Guid tripId);
        Task<bool> ValidateLocationAsync(string location, decimal latitude, decimal longitude);
    }
} 