using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Trips.Queries.GetTrips
{
    public class GetTripsQuery : IRequest<PaginatedList<TripDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchString { get; set; }
        public string SortOrder { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Guid? DriverId { get; set; }
        public Guid? VehicleId { get; set; }
    }

    public class GetTripsQueryHandler : IRequestHandler<GetTripsQuery, PaginatedList<TripDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetTripsQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<PaginatedList<TripDto>> Handle(GetTripsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Trips
                .Include(x => x.Documents)
                .Include(x => x.Ratings)
                .Include(x => x.TrackingPoints)
                .AsQueryable();

            // Apply filters based on user role
            if (!_currentUserService.IsAdmin)
            {
                query = query.Where(x => x.DriverId == _currentUserService.UserId);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchString))
            {
                query = query.Where(x =>
                    x.Notes.Contains(request.SearchString) ||
                    x.Currency.Contains(request.SearchString));
            }

            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (Enum.TryParse<Domain.Enums.TripStatus>(request.Status, true, out var status))
                {
                    query = query.Where(x => x.Status == status);
                }
            }

            if (request.StartDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt >= request.StartDate.Value);
            }

            if (request.EndDate.HasValue)
            {
                query = query.Where(x => x.CreatedAt <= request.EndDate.Value);
            }

            if (request.DriverId.HasValue)
            {
                query = query.Where(x => x.DriverId == request.DriverId.Value);
            }

            if (request.VehicleId.HasValue)
            {
                query = query.Where(x => x.VehicleId == request.VehicleId.Value);
            }

            query = request.SortOrder?.ToLower() switch
            {
                "created" => query.OrderBy(x => x.CreatedAt),
                "created_desc" => query.OrderByDescending(x => x.CreatedAt),
                "status" => query.OrderBy(x => x.Status),
                "status_desc" => query.OrderByDescending(x => x.Status),
                "departure" => query.OrderBy(x => x.EstimatedDepartureTime),
                "departure_desc" => query.OrderByDescending(x => x.EstimatedDepartureTime),
                "arrival" => query.OrderBy(x => x.EstimatedArrivalTime),
                "arrival_desc" => query.OrderByDescending(x => x.EstimatedArrivalTime),
                _ => query.OrderByDescending(x => x.CreatedAt)
            };

            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var count = await query.CountAsync(cancellationToken);

            var dtos = _mapper.Map<TripDto[]>(items);
            return new PaginatedList<TripDto>(dtos.ToList(), count, request.PageNumber, request.PageSize);
        }
    }

    public class TripDto : IMapFrom<Trip>
    {
        public Guid Id { get; set; }
        public Guid DriverId { get; set; }
        public Guid VehicleId { get; set; }
        public string StartLocation { get; set; }
        public string EndLocation { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<Trip, TripDto>();
        }
    }
} 