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
using TruckFreight.Application.Common.Models;

namespace TruckFreight.Application.Features.Payments.Commands.VerifyPayment
{
    public class VerifyPaymentCommand : IRequest<Result<PaymentGatewayResponse>>
    {
        public string Authority { get; set; }
        public decimal Amount { get; set; }
        public string Gateway { get; set; }
    }

    public class VerifyPaymentCommandValidator : AbstractValidator<VerifyPaymentCommand>
    {
        public VerifyPaymentCommandValidator()
        {
            RuleFor(x => x.Authority)
                .NotEmpty()
                .WithMessage("کد پیگیری الزامی است");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("مبلغ باید بزرگتر از صفر باشد");

            RuleFor(x => x.Gateway)
                .NotEmpty()
                .WithMessage("درگاه پرداخت الزامی است")
                .Must(x => x == "Zarinpal" || x == "NextPay" || x == "Mellat")
                .WithMessage("درگاه پرداخت نامعتبر است");
        }
    }

    public class VerifyPaymentCommandHandler : IRequestHandler<VerifyPaymentCommand, Result<PaymentGatewayResponse>>
    {
        private readonly IApplicationDbContext _db;
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<VerifyPaymentCommandHandler> _logger;

        public VerifyPaymentCommandHandler(
            IApplicationDbContext db,
            IPaymentGatewayService paymentGatewayService,
            ICurrentUserService currentUser,
            ILogger<VerifyPaymentCommandHandler> logger)
        {
            _db = db;
            _paymentGatewayService = paymentGatewayService;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<Result<PaymentGatewayResponse>> Handle(VerifyPaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUser.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return await Result<PaymentGatewayResponse>.FailAsync("کاربر یافت نشد");
                }

                var result = await _paymentGatewayService.VerifyPaymentAsync(request.Authority, request.Amount, request.Gateway);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Payment verified successfully for user {UserId}", userId);
                }
                else
                {
                    _logger.LogError("Failed to verify payment for user {UserId}: {Error}", userId, result.Error);
                }

                return result;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error verifying payment");
                return await Result<PaymentGatewayResponse>.FailAsync("خطا در تأیید پرداخت: " + ex.Message);
            }
        }
    }
} 