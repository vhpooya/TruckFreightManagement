using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Features.Administration.Queries.GetSystemOverview;
using TruckFreight.WebAdmin.Models;

namespace TruckFreight.WebAdmin.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class HomeController : BaseAdminController
    {
        public async Task<IActionResult> Index()
        {
            var query = new GetSystemOverviewQuery();
            var result = await Mediator.Send(query);

            if (result.IsSuccess)
            {
                var viewModel = new DashboardViewModel
                {
                    SystemOverview = result.Data,
                    PageTitle = "داشبورد مدیریت"
                };
                return View(viewModel);
            }

            TempData["Error"] = result.Message;
            return View(new DashboardViewModel { PageTitle = "داشبورد مدیریت" });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

/