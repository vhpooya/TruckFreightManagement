using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Trips.Queries.GetTrackingPoints
{
    public class GetTrackingPointsQuery : IRequest<Result<TrackingPointDto[]>>
    {
        public Guid TripId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }

    public class GetTrackingPointsQueryHandler : IRequestHandler<GetTrackingPointsQuery, Result<TrackingPointDto[]>>
    {
        private readonly ITrackingService _trackingService;
        private readonly IMapper _mapper;

        public GetTrackingPointsQueryHandler(
            ITrackingService trackingService,
            IMapper mapper)
        {
            _trackingService = trackingService;
            _mapper = mapper;
        }

        public async Task<Result<TrackingPointDto[]>> Handle(GetTrackingPointsQuery request, CancellationToken cancellationToken)
        {
            var result = await _trackingService.GetTripTrackingPointsAsync(
                request.TripId,
                request.StartTime,
                request.EndTime);

            if (!result.Succeeded)
            {
                return Result<TrackingPointDto[]>.Failure(result.Error);
            }

            var dtos = _mapper.Map<TrackingPointDto[]>(result.Data);
            return Result<TrackingPointDto[]>.Success(dtos);
        }
    }

    public class TrackingPointDto : IMapFrom<TripTracking>
    {
        public Guid Id { get; set; }
        public Guid TripId { get; set; }
        public string Location { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Speed { get; set; }
        public string SpeedUnit { get; set; }
        public double? FuelLevel { get; set; }
        public string FuelUnit { get; set; }
        public string Notes { get; set; }
        public DateTime Timestamp { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<TripTracking, TrackingPointDto>();
        }
    }
} 