using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Features.Payments.Queries.SearchPayments;
using TruckFreight.Application.Features.Payments.Commands.ProcessRefund;
using TruckFreight.WebAdmin.Models.Payments;

namespace TruckFreight.WebAdmin.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin,Operator")]
    public class PaymentsController : BaseAdminController
    {
        public async Task<IActionResult> Index(int page = 1, string search = "", string status = "",
                                              DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = new SearchPaymentsQuery
            {
                SearchTerm = search,
                Status = string.IsNullOrEmpty(status) ? null : Enum.Parse<Domain.Enums.PaymentStatus>(status),
                FromDate = fromDate,
                ToDate = toDate,
                PageNumber = page,
                PageSize = 20
            };

            var result = await Mediator.Send(query);

            var viewModel = new PaymentListViewModel
            {
                Payments = result.IsSuccess ? result.Data : new Application.Common.Models.PagedResponse<PaymentDto>(
                    new List<PaymentDto>(), page, 20, 0),
                SearchTerm = search,
                SelectedStatus = status,
                FromDate = fromDate,
                ToDate = toDate,
                PageTitle = "مدیریت پرداخت‌ها"
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var query = new GetPaymentDetailsQuery { PaymentId = id };
            var result = await Mediator.Send(query);

            if (!result.IsSuccess)
            {
                SetErrorMessage(result.Message);
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new PaymentDetailsViewModel
            {
                Payment = result.Data,
                PageTitle = $"جزئیات پرداخت - {result.Data.PaymentNumber}"
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessRefund(Guid id, string reason)
        {
            var command = new ProcessRefundCommand { PaymentId = id, Reason = reason };
            var result = await Mediator.Send(command);

            if (result.IsSuccess)
                SetSuccessMessage(result.Message);
            else
                SetErrorMessage(result.Message);

            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> FinancialReport()
        {
            var query =RetryPGContinueEditcsharp           var query = new GetFinancialReportQuery
           {
               FromDate = DateTime.UtcNow.AddMonths(-1),
               ToDate = DateTime.UtcNow
           };

           var result = await Mediator.Send(query);

           var viewModel = new FinancialReportViewModel
           {
               Report = result.IsSuccess ? result.Data : new FinancialReportDto(),
               PageTitle = "گزارش مالی"
           };

           return View(viewModel);
       }

       [HttpPost]
       public async Task<IActionResult> ExportReport(DateTime fromDate, DateTime toDate)
       {
           var query = new ExportFinancialReportQuery
           {
               FromDate = fromDate,
               ToDate = toDate
           };

           var result = await Mediator.Send(query);

           if (result.IsSuccess)
           {
               return File(result.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                          $"financial-report-{fromDate:yyyyMMdd}-{toDate:yyyyMMdd}.xlsx");
           }

           SetErrorMessage(result.Message);
           return RedirectToAction(nameof(FinancialReport));
       }
   }
}
