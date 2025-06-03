using System;
using MediatR;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Reports.Queries.GetTripReport
{
    public class GetTripReportQuery : IRequest<TripReportDto>
    {
        public Guid TripId { get; set; }
    }

    public class GetTripReportQueryHandler : IRequestHandler<GetTripReportQuery, TripReportDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetTripReportQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<TripReportDto> Handle(GetTripReportQuery request, CancellationToken cancellationToken)
        {
            var trip = await _context.Trips
                .Include(t => t.Driver)
                .Include(t => t.Vehicle)
                .Include(t => t.Cargo)
                .FirstOrDefaultAsync(t => t.Id == request.TripId, cancellationToken);

            if (trip == null)
            {
                throw new NotFoundException(nameof(Trip), request.TripId);
            }

            return _mapper.Map<TripReportDto>(trip);
        }
    }

    public class TripReportDto : IMapFrom<Trip>
    {
        public Guid Id { get; set; }
        public string StartLocation { get; set; }
        public string EndLocation { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string DriverName { get; set; }
        public string VehiclePlateNumber { get; set; }
        public string CargoTitle { get; set; }
        public decimal? Distance { get; set; }
        public string DistanceUnit { get; set; }
        public decimal? FuelConsumption { get; set; }
        public string FuelUnit { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<Trip, TripReportDto>()
                .ForMember(d => d.DriverName, opt => opt.MapFrom(s => s.Driver.User.FirstName + " " + s.Driver.User.LastName))
                .ForMember(d => d.VehiclePlateNumber, opt => opt.MapFrom(s => s.Vehicle.PlateNumber))
                .ForMember(d => d.CargoTitle, opt => opt.MapFrom(s => s.Cargo.Title));
        }
    }
} 