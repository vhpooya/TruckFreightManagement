using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;

namespace TruckFreight.Application.Features.Payments.Queries.GetPaymentById
{
    public class GetPaymentByIdQuery : IRequest<PaymentDto>
    {
        public string PaymentId { get; set; }
    }

    public class GetPaymentByIdQueryValidator : AbstractValidator<GetPaymentByIdQuery>
    {
        public GetPaymentByIdQueryValidator()
        {
            RuleFor(x => x.PaymentId)
                .NotEmpty().WithMessage("Payment ID is required");
        }
    }

    public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDto>
    {
        private readonly ILogger<GetPaymentByIdQueryHandler> _logger;
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetPaymentByIdQueryHandler(
            ILogger<GetPaymentByIdQueryHandler> logger,
            IApplicationDbContext context,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _context = context;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<PaymentDto> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var payment = await _context.Payments
                    .FirstOrDefaultAsync(x => x.Id == request.PaymentId, cancellationToken);

                if (payment == null)
                {
                    throw new NotFoundException(nameof(Payment), request.PaymentId);
                }

                // Only allow users to view their own payments or admins to view any payment
                if (payment.UserId != _currentUserService.UserId && !_currentUserService.IsAdmin)
                {
                    throw new ForbiddenAccessException();
                }

                return _mapper.Map<PaymentDto>(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment {PaymentId}", request.PaymentId);
                throw;
            }
        }
    }
} 