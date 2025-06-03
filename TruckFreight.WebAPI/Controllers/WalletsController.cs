using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Features.Wallets.Commands.AddFunds;
using TruckFreight.Application.Features.Wallets.Commands.WithdrawFunds;
using TruckFreight.Application.Features.Wallets.Queries.GetWalletBalance;
using TruckFreight.Application.Features.Wallets.Queries.GetWalletTransactions;

namespace TruckFreight.WebAPI.Controllers
{
    [Authorize]
    public class WalletsController : BaseController
    {
        /// <summary>
        /// Get wallet balance and information
        /// </summary>
        [HttpGet("balance")]
        public async Task<ActionResult> GetBalance()
        {
            var query = new GetWalletBalanceQuery();
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Add funds to wallet
        /// </summary>
        [HttpPost("add-funds")]
        public async Task<ActionResult> AddFunds([FromBody] AddFundsCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Withdraw funds from wallet
        /// </summary>
        [HttpPost("withdraw")]
        public async Task<ActionResult> WithdrawFunds([FromBody] WithdrawFundsCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Get wallet transaction history
        /// </summary>
        [HttpGet("transactions")]
        public async Task<ActionResult> GetTransactions([FromQuery] GetWalletTransactionsQuery query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Get transaction details
        /// </summary>
        [HttpGet("transactions/{id}")]
        public async Task<ActionResult> GetTransactionDetails(Guid id)
        {
            var query = new GetTransactionDetailsQuery { TransactionId = id };
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }
    }
}

/