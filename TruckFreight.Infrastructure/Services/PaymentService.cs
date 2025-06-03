using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PaymentService> _logger;
        private readonly IConfiguration _configuration;
        private readonly decimal _commissionRate;

        public PaymentService(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            INotificationService notificationService,
            ILogger<PaymentService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _logger = logger;
            _configuration = configuration;
            _commissionRate = _configuration.GetValue<decimal>("PaymentSettings:CommissionRate");
        }

        public async Task<Result<Payment>> CreatePaymentAsync(
            Guid tripId,
            decimal amount,
            string currency,
            string paymentMethod,
            string description = null)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(x => x.CargoRequest)
                    .FirstOrDefaultAsync(x => x.Id == tripId);

                if (trip == null)
                {
                    throw new NotFoundException(nameof(Trip), tripId);
                }

                // Validate user has permission to create payment
                if (trip.CargoRequest.CargoOwnerId.ToString() != _currentUserService.UserId &&
                    !_currentUserService.IsInRole("Admin"))
                {
                    throw new ForbiddenAccessException();
                }

                // Calculate commission
                var commissionResult = await CalculateCommissionAsync(amount);
                if (!commissionResult.Succeeded)
                {
                    return Result<Payment>.Failure(commissionResult.Error);
                }

                var payment = new Payment
                {
                    TripId = tripId,
                    Amount = amount,
                    Currency = currency,
                    PaymentMethod = paymentMethod,
                    Description = description,
                    Status = Domain.Enums.PaymentStatus.Pending,
                    Commission = commissionResult.Data,
                    CreatedBy = _currentUserService.UserId,
                    CreatedAt = DateTime.UtcNow
                };

                // Validate payment
                var validationResult = await ValidatePaymentAsync(payment);
                if (!validationResult.Succeeded)
                {
                    return Result<Payment>.Failure(validationResult.Error);
                }

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // Send notification
                await _notificationService.SendPaymentNotificationAsync(
                    payment.Id,
                    "Created",
                    $"Payment of {amount} {currency} has been created for trip {tripId}");

                return Result<Payment>.Success(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment for trip {TripId}", tripId);
                return Result<Payment>.Failure("Failed to create payment");
            }
        }

        public async Task<Result<Payment>> ProcessPaymentAsync(
            Guid paymentId,
            string transactionId,
            string status,
            string notes = null)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(x => x.Trip)
                    .ThenInclude(x => x.CargoRequest)
                    .FirstOrDefaultAsync(x => x.Id == paymentId);

                if (payment == null)
                {
                    throw new NotFoundException(nameof(Payment), paymentId);
                }

                // Validate user has permission to process payment
                if (!_currentUserService.IsInRole("Admin"))
                {
                    throw new ForbiddenAccessException();
                }

                payment.TransactionId = transactionId;
                payment.Status = Enum.Parse<Domain.Enums.PaymentStatus>(status);
                payment.Notes = notes;
                payment.ProcessedAt = DateTime.UtcNow;
                payment.ProcessedBy = _currentUserService.UserId;

                await _context.SaveChangesAsync();

                // Send notification
                await _notificationService.SendPaymentNotificationAsync(
                    payment.Id,
                    status,
                    $"Payment {paymentId} has been {status.ToLower()}");

                return Result<Payment>.Success(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment {PaymentId}", paymentId);
                return Result<Payment>.Failure("Failed to process payment");
            }
        }

        public async Task<Result<Payment>> GetPaymentAsync(Guid paymentId)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(x => x.Trip)
                    .ThenInclude(x => x.CargoRequest)
                    .FirstOrDefaultAsync(x => x.Id == paymentId);

                if (payment == null)
                {
                    throw new NotFoundException(nameof(Payment), paymentId);
                }

                // Check if user has access to this payment
                if (payment.Trip.CargoRequest.CargoOwnerId.ToString() != _currentUserService.UserId &&
                    payment.Trip.DriverId.ToString() != _currentUserService.UserId &&
                    !_currentUserService.IsInRole("Admin"))
                {
                    throw new ForbiddenAccessException();
                }

                return Result<Payment>.Success(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment {PaymentId}", paymentId);
                return Result<Payment>.Failure("Failed to retrieve payment");
            }
        }

        public async Task<Result<Payment[]>> GetTripPaymentsAsync(Guid tripId)
        {
            try
            {
                var trip = await _context.Trips
                    .Include(x => x.CargoRequest)
                    .FirstOrDefaultAsync(x => x.Id == tripId);

                if (trip == null)
                {
                    throw new NotFoundException(nameof(Trip), tripId);
                }

                // Check if user has access to this trip's payments
                if (trip.CargoRequest.CargoOwnerId.ToString() != _currentUserService.UserId &&
                    trip.DriverId.ToString() != _currentUserService.UserId &&
                    !_currentUserService.IsInRole("Admin"))
                {
                    throw new ForbiddenAccessException();
                }

                var payments = await _context.Payments
                    .Where(x => x.TripId == tripId)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToArrayAsync();

                return Result<Payment[]>.Success(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for trip {TripId}", tripId);
                return Result<Payment[]>.Failure("Failed to retrieve trip payments");
            }
        }

        public async Task<Result<Payment[]>> GetUserPaymentsAsync(string userId)
        {
            try
            {
                // Check if user has permission to view these payments
                if (userId != _currentUserService.UserId && !_currentUserService.IsInRole("Admin"))
                {
                    throw new ForbiddenAccessException();
                }

                var payments = await _context.Payments
                    .Include(x => x.Trip)
                    .ThenInclude(x => x.CargoRequest)
                    .Where(x => x.Trip.CargoRequest.CargoOwnerId.ToString() == userId ||
                               x.Trip.DriverId.ToString() == userId)
                    .OrderByDescending(x => x.CreatedAt)
                    .ToArrayAsync();

                return Result<Payment[]>.Success(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payments for user {UserId}", userId);
                return Result<Payment[]>.Failure("Failed to retrieve user payments");
            }
        }

        public async Task<Result<decimal>> CalculateCommissionAsync(decimal amount)
        {
            try
            {
                var commission = amount * _commissionRate;
                return Result<decimal>.Success(commission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating commission for amount {Amount}", amount);
                return Result<decimal>.Failure("Failed to calculate commission");
            }
        }

        public async Task<Result> ValidatePaymentAsync(Payment payment)
        {
            try
            {
                if (payment.Amount <= 0)
                {
                    return Result.Failure("Payment amount must be greater than zero");
                }

                if (string.IsNullOrEmpty(payment.Currency))
                {
                    return Result.Failure("Currency is required");
                }

                if (string.IsNullOrEmpty(payment.PaymentMethod))
                {
                    return Result.Failure("Payment method is required");
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating payment");
                return Result.Failure("Failed to validate payment");
            }
        }

        public async Task<Result> RefundPaymentAsync(Guid paymentId, string reason)
        {
            try
            {
                var payment = await _context.Payments
                    .Include(x => x.Trip)
                    .ThenInclude(x => x.CargoRequest)
                    .FirstOrDefaultAsync(x => x.Id == paymentId);

                if (payment == null)
                {
                    throw new NotFoundException(nameof(Payment), paymentId);
                }

                // Validate user has permission to refund payment
                if (!_currentUserService.IsInRole("Admin"))
                {
                    throw new ForbiddenAccessException();
                }

                if (payment.Status != Domain.Enums.PaymentStatus.Processed)
                {
                    return Result.Failure("Only processed payments can be refunded");
                }

                payment.Status = Domain.Enums.PaymentStatus.Refunded;
                payment.Notes = reason;
                payment.RefundedAt = DateTime.UtcNow;
                payment.RefundedBy = _currentUserService.UserId;

                await _context.SaveChangesAsync();

                // Send notification
                await _notificationService.SendPaymentNotificationAsync(
                    payment.Id,
                    "Refunded",
                    $"Payment {paymentId} has been refunded. Reason: {reason}");

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding payment {PaymentId}", paymentId);
                return Result.Failure("Failed to refund payment");
            }
        }

        public async Task<Result> TransferPaymentToDriverAsync(Payment payment)
        {
            try
            {
                if (payment.Status != Domain.Enums.PaymentStatus.Processed)
                {
                    throw new InvalidOperationException("Only processed payments can be transferred to driver");
                }

                // TODO: Implement transfer logic to driver's account
                payment.Status = Domain.Enums.PaymentStatus.Transferred;
                payment.LastModifiedBy = _currentUserService.UserId;
                payment.LastModifiedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                await _notificationService.SendPaymentNotificationAsync(
                    payment.Id,
                    "Transferred",
                    $"Payment has been transferred to driver's account");

                return Result.Success("Payment transferred to driver successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring payment {PaymentId} to driver", payment.Id);
                return Result.Failure("Failed to transfer payment to driver");
            }
        }
    }
} 