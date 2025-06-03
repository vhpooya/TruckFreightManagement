using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Features.Reports.Queries.GetSystemReports;
using TruckFreight.WebAdmin.Models.Reports;

namespace TruckFreight.WebAdmin.Controllers
{
   [Authorize(Roles = "Admin,SuperAdmin,Operator")]
   public class ReportsController : BaseAdminController
   {
       public async Task<IActionResult> Index()
       {
           var viewModel = new ReportsIndexViewModel
           {
               PageTitle = "گزارشات سیستم"
           };

           return View(viewModel);
       }

       public async Task<IActionResult> UserReports(DateTime? fromDate = null, DateTime? toDate = null)
       {
           var query = new GetUserReportsQuery
           {
               FromDate = fromDate ?? DateTime.UtcNow.AddMonths(-1),
               ToDate = toDate ?? DateTime.UtcNow
           };

           var result = await Mediator.Send(query);

           var viewModel = new UserReportsViewModel
           {
               Report = result.IsSuccess ? result.Data : new UserReportDto(),
               FromDate = query.FromDate,
               ToDate = query.ToDate,
               PageTitle = "گزارش کاربران"
           };

           return View(viewModel);
       }

       public async Task<IActionResult> TripReports(DateTime? fromDate = null, DateTime? toDate = null)
       {
           var query = new GetTripReportsQuery
           {
               FromDate = fromDate ?? DateTime.UtcNow.AddMonths(-1),
               ToDate = toDate ?? DateTime.UtcNow
           };

           var result = await Mediator.Send(query);

           var viewModel = new TripReportsViewModel
           {
               Report = result.IsSuccess ? result.Data : new TripReportDto(),
               FromDate = query.FromDate,
               ToDate = query.ToDate,
               PageTitle = "گزارش سفرها"
           };

           return View(viewModel);
       }

       public async Task<IActionResult> FinancialReports(DateTime? fromDate = null, DateTime? toDate = null)
       {
           var query = new GetFinancialReportsQuery
           {
               FromDate = fromDate ?? DateTime.UtcNow.AddMonths(-1),
               ToDate = toDate ?? DateTime.UtcNow
           };

           var result = await Mediator.Send(query);

           var viewModel = new FinancialReportsViewModel
           {
               Report = result.IsSuccess ? result.Data : new FinancialReportDto(),
               FromDate = query.FromDate,
               ToDate = query.ToDate,
               PageTitle = "گزارش مالی"
           };

           return View(viewModel);
       }

       [HttpPost]
       public async Task<IActionResult> ExportUserReport(DateTime fromDate, DateTime toDate)
       {
           var query = new ExportUserReportQuery
           {
               FromDate = fromDate,
               ToDate = toDate
           };

           var result = await Mediator.Send(query);

           if (result.IsSuccess)
           {
               return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                          $"user-report-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.xlsx");
           }

           SetErrorMessage(result.Message);
           return RedirectToAction(nameof(UserReports));
       }

       [HttpPost]
       public async Task<IActionResult> ExportTripReport(DateTime fromDate, DateTime toDate)
       {
           var query = new ExportTripReportQuery
           {
               FromDate = fromDate,
               ToDate = toDate
           };

           var result = await Mediator.Send(query);

           if (result.IsSuccess)
           {
               return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                          $"trip-report-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.xlsx");
           }

           SetErrorMessage(result.Message);
           return RedirectToAction(nameof(TripReports));
       }
   }
}
