using System;
using MediatR;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Users.Queries.GetUsers
{
    public class GetUsersQuery : IRequest<PaginatedList<UserDto>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SearchString { get; set; }
        public string Role { get; set; }
    }

    public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedList<UserDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetUsersQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaginatedList<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Users.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(request.SearchString))
            {
                query = query.Where(u => u.UserName.Contains(request.SearchString) || u.Email.Contains(request.SearchString) || u.FirstName.Contains(request.SearchString) || u.LastName.Contains(request.SearchString));
            }
            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                query = query.Where(u => u.Role == request.Role);
            }
            return await query
                .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
                .PaginatedListAsync(request.PageNumber, request.PageSize);
        }
    }

    public class UserDto : IMapFrom<User>
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public void Mapping(Profile profile)
        {
            profile.CreateMap<User, UserDto>();
        }
    }
} 