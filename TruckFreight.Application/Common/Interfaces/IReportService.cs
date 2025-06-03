using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TruckFreight.Domain.Entities;
using TruckFreight.Application.Features.Reports.Queries.GetTripReport;

namespace TruckFreight.Application.Common.Interfaces
{
    public interface IReportService
    {
        Task<TripReportDto> GenerateTripReportAsync(Guid tripId);
        Task<List<Payment>> GeneratePaymentReportAsync(DateTime startDate, DateTime endDate);
        Task<List<Trip>> GenerateDriverReportAsync(Guid driverId, DateTime startDate, DateTime endDate);
        Task<List<CargoRequest>> GenerateCargoReportAsync(DateTime startDate, DateTime endDate);
    }
} 