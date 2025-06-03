using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Payments.Commands.UpdatePaymentStatus
{
    public class UpdatePaymentStatusCommand : IRequest<Result>
    {
        public Guid Id { get; set; }
        public PaymentStatus Status { get; set; }
        public string Notes { get; set; }
    }

    public class UpdatePaymentStatusCommandValidator : AbstractValidator<UpdatePaymentStatusCommand>
    {
        public UpdatePaymentStatusCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Status).IsInEnum();
            RuleFor(x => x.Notes).MaximumLength(500);
        }
    }

    public class UpdatePaymentStatusCommandHandler : IRequestHandler<UpdatePaymentStatusCommand, Result>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;

        public UpdatePaymentStatusCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            INotificationService notificationService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
        }

        public async Task<Result> Handle(UpdatePaymentStatusCommand request, CancellationToken cancellationToken)
        {
            var payment = await _context.Payments
                .Include(x => x.Trip)
                .FirstOrDefaultAsync(x => x.Id == request.Id);

            if (payment == null)
            {
                throw new NotFoundException(nameof(Payment), request.Id);
            }

            if (payment.Status == request.Status)
            {
                return Result.Success("Payment status is already set to the requested status");
            }

            payment.Status = request.Status;
            payment.Notes = request.Notes;
            payment.LastModifiedBy = _currentUserService.UserId;
            payment.LastModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Send notification
            await _notificationService.SendPaymentNotificationAsync(
                payment.Id,
                "StatusUpdated",
                $"Payment status has been updated to {request.Status}");

            return Result.Success("Payment status updated successfully");
        }
    }
} 