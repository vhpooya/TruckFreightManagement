using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Payments.Commands.ProcessPayment
{
    public class ProcessPaymentCommand : IRequest<Result<Guid>>
    {
        public Guid PaymentId { get; set; }
        public string TransactionId { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }

    public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
    {
        public ProcessPaymentCommandValidator()
        {
            RuleFor(x => x.PaymentId).NotEmpty();
            RuleFor(x => x.TransactionId).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Status).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Notes).MaximumLength(500);
        }
    }

    public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Result<Guid>>
    {
        private readonly IPaymentService _paymentService;
        private readonly ICurrentUserService _currentUserService;

        public ProcessPaymentCommandHandler(
            IPaymentService paymentService,
            ICurrentUserService currentUserService)
        {
            _paymentService = paymentService;
            _currentUserService = currentUserService;
        }

        public async Task<Result<Guid>> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
        {
            var result = await _paymentService.ProcessPaymentAsync(
                request.PaymentId,
                request.TransactionId,
                request.Status,
                request.Notes);

            if (!result.Succeeded)
            {
                return Result<Guid>.Failure(result.Error);
            }

            return Result<Guid>.Success(result.Data.Id, "Payment processed successfully");
        }
    }
} 