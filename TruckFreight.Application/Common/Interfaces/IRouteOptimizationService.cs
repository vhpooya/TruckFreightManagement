using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Common.Interfaces
{
    public interface IRouteOptimizationService
    {
        Task<List<string>> OptimizeRouteAsync(string startLocation, string endLocation, Vehicle vehicle);
        Task<decimal> CalculateDistanceAsync(string startLocation, string endLocation);
        Task<decimal> CalculateEstimatedTimeAsync(string startLocation, string endLocation, Vehicle vehicle);
        Task<bool> ValidateRouteAsync(string startLocation, string endLocation, Vehicle vehicle);
    }
} 