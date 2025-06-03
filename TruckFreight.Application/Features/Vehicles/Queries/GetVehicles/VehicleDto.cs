using System;
using TruckFreight.Application.Common.Mappings;
using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Enums;

namespace TruckFreight.Application.Features.Vehicles.Queries.GetVehicles
{
    public class VehicleDto : IMapFrom<Vehicle>
    {
        public Guid Id { get; set; }
        public string PlateNumber { get; set; }
        public VehicleType Type { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Color { get; set; }
        public string VIN { get; set; }
        public string EngineNumber { get; set; }
        public decimal Capacity { get; set; }
        public string CapacityUnit { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public string DimensionUnit { get; set; }
        public bool IsActive { get; set; }
        public DateTime? InsuranceExpiryDate { get; set; }
        public DateTime? RegistrationExpiryDate { get; set; }
        public DateTime? InspectionExpiryDate { get; set; }
        public bool IsInsuranceValid { get; set; }
        public bool IsRegistrationValid { get; set; }
        public bool IsInspectionValid { get; set; }
        public bool IsFullyCompliant { get; set; }

        public void Mapping(AutoMapper.Profile profile)
        {
            profile.CreateMap<Vehicle, VehicleDto>()
                .ForMember(d => d.IsInsuranceValid, opt => opt.MapFrom(s => s.IsInsuranceValid))
                .ForMember(d => d.IsRegistrationValid, opt => opt.MapFrom(s => s.IsRegistrationValid))
                .ForMember(d => d.IsInspectionValid, opt => opt.MapFrom(s => s.IsInspectionValid))
                .ForMember(d => d.IsFullyCompliant, opt => opt.MapFrom(s => s.IsFullyCompliant));
        }
    }
} 