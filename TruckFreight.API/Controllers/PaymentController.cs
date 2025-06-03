using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Payments.Commands.InitiatePayment;
using TruckFreight.Application.Features.Payments.Commands.RefundPayment;
using TruckFreight.Application.Features.Payments.Commands.VerifyPayment;
using TruckFreight.Application.Features.Payments.Queries.GetPaymentById;
using TruckFreight.Application.Features.Payments.Queries.GetPaymentsByUserId;
using TruckFreight.Application.Features.Payments.Commands.CreatePayment;
using TruckFreight.Application.Features.Payments.DTOs;
using TruckFreight.Application.Features.Payments.Queries.GetPaymentStatus;
using TruckFreight.Application.Features.Payments.Queries.GetTransactions;

namespace TruckFreight.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ApiControllerBase
    {
        private readonly ICurrentUserService _currentUserService;

        public PaymentController(ICurrentUserService currentUserService)
        {
            _currentUserService = currentUserService;
        }

        [HttpPost("initiate")]
        public async Task<ActionResult<PaymentInfo>> InitiatePayment(InitiatePaymentCommand command)
        {
            // Set the user ID from the current user
            command.UserId = _currentUserService.UserId;

            var result = await Mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("verify/{paymentId}")]
        public async Task<ActionResult<PaymentStatus>> VerifyPayment(string paymentId)
        {
            var command = new VerifyPaymentCommand { PaymentId = paymentId };
            var result = await Mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("refund/{paymentId}")]
        public async Task<ActionResult> RefundPayment(string paymentId, [FromBody] decimal amount)
        {
            var command = new RefundPaymentCommand { PaymentId = paymentId, Amount = amount };
            var result = await Mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("{paymentId}")]
        public async Task<ActionResult<PaymentDto>> GetPayment(string paymentId)
        {
            var query = new GetPaymentByIdQuery { PaymentId = paymentId };
            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<PaginatedList<PaymentDto>>> GetUserPayments(
            string userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            // Only allow users to view their own payments
            if (userId != _currentUserService.UserId)
            {
                return Forbid();
            }

            var query = new GetPaymentsByUserIdQuery
            {
                UserId = userId,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var result = await Mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<PaymentGatewayResponse>> CreatePayment(CreatePaymentCommand command)
        {
            return await Mediator.Send(command);
        }

        [HttpPost("verify")]
        public async Task<ActionResult<PaymentGatewayResponse>> VerifyPayment(VerifyPaymentCommand command)
        {
            return await Mediator.Send(command);
        }

        [HttpGet("status/{paymentId}")]
        public async Task<ActionResult<PaymentGatewayResponse>> GetPaymentStatus(string paymentId)
        {
            return await Mediator.Send(new GetPaymentStatusQuery { PaymentId = paymentId });
        }

        [HttpGet("transactions")]
        public async Task<ActionResult<PaginatedList<TransactionDto>>> GetTransactions([FromQuery] GetTransactionsQuery query)
        {
            return await Mediator.Send(query);
        }
    }
} 