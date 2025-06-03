using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Features.Payments.Commands.InitiatePayment;
using TruckFreight.Application.Features.Payments.Commands.VerifyPayment;
using TruckFreight.Application.Features.Payments.Queries.GetUserPayments;
using TruckFreight.Application.Features.Payments.Queries.GetPaymentDetails;

namespace TruckFreight.WebAPI.Controllers
{
    [Authorize]
    public class PaymentsController : BaseController
    {
        /// <summary>
        /// Initiate payment for a trip
        /// </summary>
        [HttpPost("initiate")]
        [Authorize(Roles = "CargoOwner")]
        public async Task<ActionResult> InitiatePayment([FromBody] InitiatePaymentCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Verify payment (callback from payment gateway)
        /// </summary>
        [HttpPost("verify")]
        [AllowAnonymous]
        public async Task<ActionResult> VerifyPayment([FromBody] VerifyPaymentCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Get user's payment history
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetUserPayments([FromQuery] GetUserPaymentsQuery query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Get payment details
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult> GetDetails(Guid id)
        {
            var query = new GetPaymentDetailsQuery { PaymentId = id };
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Request payment refund
        /// </summary>
        [HttpPost("{id}/refund")]
        public async Task<ActionResult> RequestRefund(Guid id, [FromBody] RequestRefundCommand command)
        {
            command.PaymentId = id;
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
    }
}

/