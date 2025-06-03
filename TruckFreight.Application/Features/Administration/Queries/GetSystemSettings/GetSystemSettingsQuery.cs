using System;
using MediatR;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Administration.Queries.GetSystemSettings
{
    public class GetSystemSettingsQuery : IRequest<PaginatedList<SystemSettingsDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchString { get; set; }
    }

    public class GetSystemSettingsQueryHandler : IRequestHandler<GetSystemSettingsQuery, PaginatedList<SystemSettingsDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetSystemSettingsQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<SystemSettingsDto>> Handle(GetSystemSettingsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.SystemSettings.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.SearchString))
            {
                query = query.Where(s => s.SettingKey.Contains(request.SearchString) || s.SettingValue.Contains(request.SearchString));
            }
            return await query
                .ProjectTo<SystemSettingsDto>(_mapper.ConfigurationProvider)
                .PaginatedListAsync(request.PageNumber, request.PageSize);
        }
    }

    public class SystemSettingsDto : IMapFrom<SystemSettings>
    {
        public Guid Id { get; set; }
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<SystemSettings, SystemSettingsDto>();
        }
    }
} 