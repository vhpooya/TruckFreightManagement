using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;

namespace TruckFreight.Application.Features.Payments.Commands.RefundPayment
{
    public class RefundPaymentCommand : IRequest<Result>
    {
        public string PaymentId { get; set; }
        public decimal Amount { get; set; }
    }

    public class RefundPaymentCommandValidator : AbstractValidator<RefundPaymentCommand>
    {
        public RefundPaymentCommandValidator()
        {
            RuleFor(x => x.PaymentId)
                .NotEmpty().WithMessage("Payment ID is required");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0");
        }
    }

    public class RefundPaymentCommandHandler : IRequestHandler<RefundPaymentCommand, Result>
    {
        private readonly ILogger<RefundPaymentCommandHandler> _logger;
        private readonly IPaymentGatewayService _paymentGatewayService;

        public RefundPaymentCommandHandler(
            ILogger<RefundPaymentCommandHandler> logger,
            IPaymentGatewayService paymentGatewayService)
        {
            _logger = logger;
            _paymentGatewayService = paymentGatewayService;
        }

        public async Task<Result> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _paymentGatewayService.RefundPaymentAsync(request.PaymentId, request.Amount);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding payment {PaymentId}", request.PaymentId);
                return Result.Failure("Failed to refund payment");
            }
        }
    }
} 