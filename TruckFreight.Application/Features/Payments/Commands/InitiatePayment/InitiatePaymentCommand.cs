using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;

namespace TruckFreight.Application.Features.Payments.Commands.InitiatePayment
{
    public class InitiatePaymentCommand : IRequest<Result<PaymentInfo>>
    {
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class InitiatePaymentCommandValidator : AbstractValidator<InitiatePaymentCommand>
    {
        public InitiatePaymentCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^09[0-9]{9}$").WithMessage("Invalid phone number format");
        }
    }

    public class InitiatePaymentCommandHandler : IRequestHandler<InitiatePaymentCommand, Result<PaymentInfo>>
    {
        private readonly ILogger<InitiatePaymentCommandHandler> _logger;
        private readonly IPaymentGatewayService _paymentGatewayService;

        public InitiatePaymentCommandHandler(
            ILogger<InitiatePaymentCommandHandler> logger,
            IPaymentGatewayService paymentGatewayService)
        {
            _logger = logger;
            _paymentGatewayService = paymentGatewayService;
        }

        public async Task<Result<PaymentInfo>> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var paymentRequest = new PaymentRequest
                {
                    UserId = request.UserId,
                    Amount = request.Amount,
                    Currency = request.Currency,
                    Description = request.Description,
                    PhoneNumber = request.PhoneNumber
                };

                var result = await _paymentGatewayService.InitiatePaymentAsync(paymentRequest);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating payment for user {UserId}", request.UserId);
                return Result<PaymentInfo>.Failure("Failed to initiate payment");
            }
        }
    }
} 