using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.Application.Features.Administration.Queries.GetSystemHealth;
using TruckFreight.Application.Features.Administration.Commands.ClearCache;
using TruckFreight.WebAdmin.Models.System;

namespace TruckFreight.WebAdmin.Controllers
{
    [Authorize(Roles = "SuperAdmin")]
    public class SystemController : BaseAdminController
    {
        public async Task<IActionResult> Index()
        {
            var query = new GetSystemHealthQuery();
            var result = await Mediator.Send(query);

            var viewModel = new SystemHealthViewModel
            {
                HealthData = result.IsSuccess ? result.Data : new SystemHealthDto(),
                PageTitle = "سلامت سیستم"
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> ClearCache()
        {
            var command = new ClearCacheCommand();
            var result = await Mediator.Send(command);

            if (result.IsSuccess)
                SetSuccessMessage("کش سیستم پاک شد");
            else
                SetErrorMessage(result.Message);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> RestartServices()
        {
            var command = new RestartServicesCommand();
            var result = await Mediator.Send(command);

            if (result.IsSuccess)
                SetSuccessMessage("سرویس‌ها مجدداً راه‌اندازی شدند");
            else
                SetErrorMessage(result.Message);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Logs()
        {
            var query = new GetSystemLogsQuery { PageSize = 100 };
            var result = await Mediator.Send(query);

            var viewModel = new SystemLogsViewModel
            {
                Logs = result.IsSuccess ? result.Data : new List<SystemLogDto>(),
                PageTitle = "لاگ‌های سیستم"
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> BackupDatabase()
        {
            var command = new BackupDatabaseCommand();
            var result = await Mediator.Send(command);

            if (result.IsSuccess)
            {
                SetSuccessMessage("پشتیبان‌گیری انجام شد");
                return File(result.Data, "application/octet-stream", $"backup-{DateTime.UtcNow:yyyyMMdd-HHmmss}.sql");
            }

            SetErrorMessage(result.Message);
            return RedirectToAction(nameof(Index));
        }
    }
}
Additional View Models
csharp/