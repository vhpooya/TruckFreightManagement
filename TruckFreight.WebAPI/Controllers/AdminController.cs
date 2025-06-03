using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Features.Administration.Commands.SendBroadcastNotification;
using TruckFreight.Application.Features.Administration.Queries.GetSystemLogs;

namespace TruckFreight.WebAPI.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : BaseController
    {
        /// <summary>
        /// Send broadcast notification to all users or specific role
        /// </summary>
        [HttpPost("broadcast-notification")]
        public async Task<ActionResult> SendBroadcastNotification([FromBody] SendBroadcastNotificationCommand command)
        {
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }

        /// <summary>
        /// Get system logs
        /// </summary>
        [HttpGet("system-logs")]
        public async Task<ActionResult> GetSystemLogs([FromQuery] GetSystemLogsQuery query)
        {
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Get system health status
        /// </summary>
        [HttpGet("health-status")]
        public async Task<ActionResult> GetHealthStatus()
        {
            var query = new GetSystemHealthQuery();
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        /// <summary>
        /// Backup database
        /// </summary>
        [HttpPost("backup-database")]
        public async Task<ActionResult> BackupDatabase()
        {
            var command = new BackupDatabaseCommand();
            var result = await Mediator.Send(command);
            return HandleResult(result);
        }
    }
}

/