using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Features.Reports.Queries.GetDriverStatistics;
using TruckFreight.Application.Features.Reports.Queries.GetCargoOwnerStatistics;

namespace TruckFreight.WebAPI.Controllers
{
   [Authorize]
   public class ReportsController : BaseController
   {
       /// <summary>
       /// Get driver statistics and reports
       /// </summary>
       [HttpGet("driver-statistics")]
       [Authorize(Roles = "Driver")]
       public async Task<ActionResult> GetDriverStatistics([FromQuery] GetDriverStatisticsQuery query)
       {
           var result = await Mediator.Send(query);
           return HandleResult(result);
       }

       /// <summary>
       /// Get cargo owner statistics and reports
       /// </summary>
       [HttpGet("cargo-owner-statistics")]
       [Authorize(Roles = "CargoOwner")]
       public async Task<ActionResult> GetCargoOwnerStatistics([FromQuery] GetCargoOwnerStatisticsQuery query)
       {
           var result = await Mediator.Send(query);
           return HandleResult(result);
       }

       /// <summary>
       /// Export trip report
       /// </summary>
       [HttpGet("trips/export")]
       public async Task<ActionResult> ExportTripReport([FromQuery] ExportTripReportQuery query)
       {
           var result = await Mediator.Send(query);
           return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                      $"trip-report-{DateTime.UtcNow:yyyyMMdd}.xlsx");
       }
   }
}
API Configuration and Middleware
csharp/