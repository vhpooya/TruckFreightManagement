using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Features.Payments.DTOs;
using TruckFreight.Domain.Entities;
using TruckFreight.Shared.Wrapper;

namespace TruckFreight.Application.Features.Payments.Commands.CreatePayment
{
    public class CreatePaymentCommand : IRequest<Result<PaymentResultDto>>
    {
        public CreatePaymentDto Payment { get; set; }
    }

    public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
    {
        public CreatePaymentCommandValidator()
        {
            RuleFor(x => x.Payment).NotNull();
            RuleFor(x => x.Payment.DeliveryId).NotEmpty();
            RuleFor(x => x.Payment.Amount).GreaterThan(0);
            RuleFor(x => x.Payment.Gateway).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Payment.CallbackUrl).NotEmpty().MaximumLength(500);
            RuleFor(x => x.Payment.PayerId).NotEmpty();
        }
    }

    public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Result<PaymentResultDto>>
    {
        private readonly IApplicationDbContext _db;
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<CreatePaymentCommandHandler> _logger;

        public CreatePaymentCommandHandler(
            IApplicationDbContext db,
            IPaymentGatewayService paymentGatewayService,
            ICurrentUserService currentUser,
            ILogger<CreatePaymentCommandHandler> logger)
        {
            _db = db;
            _paymentGatewayService = paymentGatewayService;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<Result<PaymentResultDto>> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // اعتبارسنجی کاربر
                var userId = _currentUser.UserId;
                if (string.IsNullOrEmpty(userId) || userId != request.Payment.PayerId)
                    return await Result<PaymentResultDto>.FailAsync("دسترسی غیرمجاز یا کاربر یافت نشد.");

                // بررسی وجود تحویل بار
                var delivery = await _db.Deliveries.FirstOrDefaultAsync(x => x.Id == request.Payment.DeliveryId, cancellationToken);
                if (delivery == null)
                    return await Result<PaymentResultDto>.FailAsync("درخواست تحویل بار یافت نشد.");

                // ایجاد رکورد پرداخت اولیه
                var payment = new Payment
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Amount = request.Payment.Amount,
                    Currency = "IRR",
                    Description = request.Payment.Description,
                    Status = Domain.Enums.PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    DeliveryId = request.Payment.DeliveryId,
                    Gateway = request.Payment.Gateway
                };
                _db.Payments.Add(payment);
                await _db.SaveChangesAsync(cancellationToken);

                // فراخوانی سرویس درگاه پرداخت
                var gatewayResult = await _paymentGatewayService.CreatePaymentAsync(new PaymentRequest
                {
                    UserId = userId,
                    Amount = request.Payment.Amount,
                    Currency = "IRR",
                    Description = request.Payment.Description,
                    PhoneNumber = _currentUser.PhoneNumber
                }, request.Payment.Gateway, request.Payment.CallbackUrl);

                if (!gatewayResult.Success)
                {
                    payment.Status = Domain.Enums.PaymentStatus.Failed;
                    payment.UpdatedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync(cancellationToken);
                    return await Result<PaymentResultDto>.FailAsync(gatewayResult.Message);
                }

                // به‌روزرسانی اطلاعات پرداخت با اطلاعات درگاه
                payment.GatewayToken = gatewayResult.Data.GatewayToken;
                payment.ReferenceId = gatewayResult.Data.PaymentId;
                payment.Status = Domain.Enums.PaymentStatus.Pending;
                payment.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(cancellationToken);

                var result = new PaymentResultDto
                {
                    PaymentId = payment.Id,
                    Gateway = payment.Gateway,
                    Authority = payment.GatewayToken,
                    PaymentUrl = gatewayResult.Data.RedirectUrl,
                    Status = payment.Status.ToString(),
                    Message = "لینک پرداخت با موفقیت ایجاد شد.",
                    CreatedAt = payment.CreatedAt
                };
                return await Result<PaymentResultDto>.SuccessAsync(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ایجاد پرداخت");
                return await Result<PaymentResultDto>.FailAsync("خطا در ایجاد پرداخت: " + ex.Message);
            }
        }
    }
} 