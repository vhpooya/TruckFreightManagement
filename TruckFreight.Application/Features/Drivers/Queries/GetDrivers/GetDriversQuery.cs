using System;
using MediatR;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Drivers.Queries.GetDrivers
{
    public class GetDriversQuery : IRequest<PaginatedList<DriverDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchString { get; set; }
        public string LicenseType { get; set; }
    }

    public class GetDriversQueryHandler : IRequestHandler<GetDriversQuery, PaginatedList<DriverDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetDriversQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<DriverDto>> Handle(GetDriversQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Drivers.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.SearchString))
            {
                query = query.Where(d => d.LicenseNumber.Contains(request.SearchString) || d.NationalId.Contains(request.SearchString));
            }
            if (!string.IsNullOrWhiteSpace(request.LicenseType))
            {
                query = query.Where(d => d.LicenseType == request.LicenseType);
            }
            return await query
                .ProjectTo<DriverDto>(_mapper.ConfigurationProvider)
                .PaginatedListAsync(request.PageNumber, request.PageSize);
        }
    }

    public class DriverDto : IMapFrom<Driver>
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string LicenseNumber { get; set; }
        public DateTime LicenseExpiryDate { get; set; }
        public string LicenseType { get; set; }
        public string NationalId { get; set; }
        public string Address { get; set; }
        public string EmergencyContact { get; set; }
        public DateTime CreatedAt { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<Driver, DriverDto>();
        }
    }
} 