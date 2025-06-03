using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Payments.Queries.GetPayment
{
    public class GetPaymentQuery : IRequest<Result<PaymentDto>>
    {
        public Guid Id { get; set; }
    }

    public class GetPaymentQueryHandler : IRequestHandler<GetPaymentQuery, Result<PaymentDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public GetPaymentQueryHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<Result<PaymentDto>> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
        {
            var payment = await _context.Payments
                .Include(x => x.Trip)
                .ThenInclude(x => x.Driver)
                .Include(x => x.Trip)
                .ThenInclude(x => x.CargoRequest)
                .ThenInclude(x => x.CargoOwner)
                .FirstOrDefaultAsync(x => x.Id == request.Id);

            if (payment == null)
            {
                throw new NotFoundException(nameof(Payment), request.Id);
            }

            // Check if user has access to this payment
            if (payment.Trip.DriverId.ToString() != _currentUserService.UserId &&
                payment.Trip.CargoRequest.CargoOwnerId.ToString() != _currentUserService.UserId &&
                !_currentUserService.IsInRole("Admin"))
            {
                throw new ForbiddenAccessException();
            }

            var dto = _mapper.Map<PaymentDto>(payment);
            return Result<PaymentDto>.Success(dto);
        }
    }

    public class PaymentDto : IMapFrom<Payment>
    {
        public Guid Id { get; set; }
        public Guid TripId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        public PaymentStatus Status { get; set; }
        public string Notes { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }

        public void Mapping(Profile profile)
        {
            profile.CreateMap<Payment, PaymentDto>();
        }
    }
} 