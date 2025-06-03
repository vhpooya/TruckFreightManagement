using AutoMapper;
using TruckFreight.Application.Features.CargoRequests.Queries.GetCargoRequests;
using TruckFreight.Application.Features.Drivers.Queries.GetDrivers;
using TruckFreight.Application.Features.Notifications.Queries.GetNotifications;
using TruckFreight.Application.Features.Payments.Queries.GetPayments;
using TruckFreight.Application.Features.Reports.Queries.GetTripReport;
using TruckFreight.Application.Features.Trips.Queries.GetTrips;
using TruckFreight.Application.Features.Users.Queries.GetUsers;
using TruckFreight.Application.Features.Vehicles.Queries.GetVehicles;
using TruckFreight.Application.Features.Administration.Queries.GetSystemSettings;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CargoRequest, CargoRequestDto>();
            CreateMap<Driver, DriverDto>();
            CreateMap<Notification, NotificationDto>();
            CreateMap<Payment, PaymentDto>();
            CreateMap<Trip, TripDto>();
            CreateMap<Trip, TripReportDto>();
            CreateMap<User, UserDto>();
            CreateMap<Vehicle, VehicleDto>();
            CreateMap<SystemSettings, SystemSettingsDto>();
        }
    }
} 