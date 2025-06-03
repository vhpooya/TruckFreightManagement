using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Shared.Wrapper;

namespace TruckFreight.Application.Features.Payments.Queries.GetPaymentStatus
{
    public class GetPaymentStatusQuery : IRequest<Result<PaymentGatewayResponse>>
    {
        public string PaymentId { get; set; }
        public string Gateway { get; set; }
    }

    public class GetPaymentStatusQueryValidator : AbstractValidator<GetPaymentStatusQuery>
    {
        public GetPaymentStatusQueryValidator()
        {
            RuleFor(x => x.PaymentId)
                .NotEmpty()
                .WithMessage("شناسه پرداخت الزامی است");

            RuleFor(x => x.Gateway)
                .NotEmpty()
                .WithMessage("درگاه پرداخت الزامی است")
                .Must(x => x == "Zarinpal" || x == "NextPay" || x == "Mellat")
                .WithMessage("درگاه پرداخت نامعتبر است");
        }
    }

    public class GetPaymentStatusQueryHandler : IRequestHandler<GetPaymentStatusQuery, Result<PaymentGatewayResponse>>
    {
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetPaymentStatusQueryHandler> _logger;

        public GetPaymentStatusQueryHandler(
            IPaymentGatewayService paymentGatewayService,
            ICurrentUserService currentUserService,
            ILogger<GetPaymentStatusQueryHandler> logger)
        {
            _paymentGatewayService = paymentGatewayService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<PaymentGatewayResponse>> Handle(GetPaymentStatusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return await Result<PaymentGatewayResponse>.FailAsync("کاربر یافت نشد");
                }

                var result = await _paymentGatewayService.GetPaymentStatusAsync(request.PaymentId, request.Gateway);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Payment status retrieved successfully for user {UserId}", userId);
                }
                else
                {
                    _logger.LogError("Failed to retrieve payment status for user {UserId}: {Error}", userId, result.Error);
                }

                return result;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving payment status");
                return await Result<PaymentGatewayResponse>.FailAsync("خطا در دریافت وضعیت پرداخت: " + ex.Message);
            }
        }
    }
} 