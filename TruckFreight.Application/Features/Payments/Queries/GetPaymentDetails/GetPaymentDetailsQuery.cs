using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Payments.DTOs;

namespace TruckFreight.Application.Features.Payments.Queries.GetPaymentDetails
{
    public class GetPaymentDetailsQuery : IRequest<Result<PaymentDetailsDto>>
    {
        public Guid PaymentId { get; set; }
    }

    public class GetPaymentDetailsQueryValidator : AbstractValidator<GetPaymentDetailsQuery>
    {
        public GetPaymentDetailsQueryValidator()
        {
            RuleFor(x => x.PaymentId)
                .NotEmpty().WithMessage("Payment ID is required");
        }
    }

    public class GetPaymentDetailsQueryHandler : IRequestHandler<GetPaymentDetailsQuery, Result<PaymentDetailsDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetPaymentDetailsQueryHandler> _logger;

        public GetPaymentDetailsQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetPaymentDetailsQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<PaymentDetailsDto>> Handle(GetPaymentDetailsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<PaymentDetailsDto>.Failure("User not authenticated");
                }

                // Get payment with related data
                var payment = await _context.Payments
                    .Include(p => p.Delivery)
                        .ThenInclude(d => d.Driver)
                    .Include(p => p.Delivery)
                        .ThenInclude(d => d.CargoRequest)
                            .ThenInclude(cr => cr.CargoOwner)
                    .Include(p => p.PaymentHistories)
                    .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

                if (payment == null)
                {
                    return Result<PaymentDetailsDto>.Failure("Payment not found");
                }

                // Check if user is authorized to view this payment
                var isAuthorized = payment.Delivery.Driver.UserId == userId || // Driver
                                 payment.Delivery.CargoRequest.CargoOwner.UserId == userId || // Cargo owner
                                 _currentUserService.IsInRole("Admin"); // Admin

                if (!isAuthorized)
                {
                    return Result<PaymentDetailsDto>.Failure("You are not authorized to view this payment");
                }

                var result = new PaymentDetailsDto
                {
                    Id = payment.Id,
                    DeliveryId = payment.DeliveryId,
                    Amount = payment.Amount,
                    PaymentMethod = payment.PaymentMethod,
                    TransactionReference = payment.TransactionReference,
                    Status = payment.Status,
                    Description = payment.Description,
                    CreatedAt = payment.CreatedAt,
                    CompletedAt = payment.CompletedAt,
                    DriverName = $"{payment.Delivery.Driver.FirstName} {payment.Delivery.Driver.LastName}",
                    CargoOwnerName = $"{payment.Delivery.CargoRequest.CargoOwner.FirstName} {payment.Delivery.CargoRequest.CargoOwner.LastName}",
                    DriverId = payment.Delivery.Driver.UserId,
                    CargoOwnerId = payment.Delivery.CargoRequest.CargoOwner.UserId,
                    DriverPhone = payment.Delivery.Driver.PhoneNumber,
                    CargoOwnerPhone = payment.Delivery.CargoRequest.CargoOwner.PhoneNumber,
                    DriverEmail = payment.Delivery.Driver.Email,
                    CargoOwnerEmail = payment.Delivery.CargoRequest.CargoOwner.Email,
                    DeliveryReference = payment.Delivery.ReferenceNumber,
                    PickupLocation = payment.Delivery.CargoRequest.PickupLocation,
                    DeliveryLocation = payment.Delivery.CargoRequest.DeliveryLocation,
                    PaymentHistory = payment.PaymentHistories
                        .OrderByDescending(h => h.Timestamp)
                        .Select(h => new PaymentHistoryDto
                        {
                            Status = h.Status,
                            Description = h.Description,
                            Timestamp = h.Timestamp,
                            UpdatedBy = h.UpdatedBy
                        })
                        .ToList()
                };

                return Result<PaymentDetailsDto>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment details");
                return Result<PaymentDetailsDto>.Failure("Error getting payment details");
            }
        }
    }
} 