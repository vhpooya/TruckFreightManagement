using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;

namespace TruckFreight.Application.Features.Payments.Queries.GetPaymentsByUserId
{
    public class GetPaymentsByUserIdQuery : IRequest<PaginatedList<PaymentDto>>
    {
        public string UserId { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class GetPaymentsByUserIdQueryValidator : AbstractValidator<GetPaymentsByUserIdQuery>
    {
        public GetPaymentsByUserIdQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");
        }
    }

    public class GetPaymentsByUserIdQueryHandler : IRequestHandler<GetPaymentsByUserIdQuery, PaginatedList<PaymentDto>>
    {
        private readonly ILogger<GetPaymentsByUserIdQueryHandler> _logger;
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetPaymentsByUserIdQueryHandler(
            ILogger<GetPaymentsByUserIdQueryHandler> logger,
            IApplicationDbContext context,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<PaginatedList<PaymentDto>> Handle(GetPaymentsByUserIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Only allow users to view their own payments or admins to view any user's payments
                if (request.UserId != _currentUserService.UserId && !_currentUserService.IsAdmin)
                {
                    throw new ForbiddenAccessException();
                }

                var query = _context.Payments
                    .Where(x => x.UserId == request.UserId)
                    .OrderByDescending(x => x.CreatedAt);

                var payments = await query
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync(cancellationToken);

                var totalCount = await query.CountAsync(cancellationToken);

                var paymentDtos = _mapper.Map<PaymentDto[]>(payments);

                return new PaginatedList<PaymentDto>(
                    paymentDtos,
                    totalCount,
                    request.PageNumber,
                    request.PageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for user {UserId}", request.UserId);
                throw;
            }
        }
    }
} 