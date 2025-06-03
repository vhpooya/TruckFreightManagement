using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruckFreight.WebAdmin.Models.ViewModels;
using TruckFreight.WebAdmin.Services;

namespace TruckFreight.WebAdmin.Controllers
{
    [Authorize]
    public class CargoOwnersController : Controller
    {
        private readonly ILogger<CargoOwnersController> _logger;
        private readonly ICargoOwnerService _cargoOwnerService;

        public CargoOwnersController(
            ILogger<CargoOwnersController> logger,
            ICargoOwnerService cargoOwnerService)
        {
            _logger = logger;
            _cargoOwnerService = cargoOwnerService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var cargoOwners = await _cargoOwnerService.GetAllCargoOwnersAsync();
                return View(cargoOwners);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching cargo owners");
                return View("Error");
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var cargoOwner = await _cargoOwnerService.GetCargoOwnerByIdAsync(id);
                if (cargoOwner == null)
                {
                    return NotFound();
                }
                return View(cargoOwner);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching cargo owner details");
                return View("Error");
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CargoOwnerDto cargoOwner)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _cargoOwnerService.CreateCargoOwnerAsync(cargoOwner);
                    return RedirectToAction(nameof(Index));
                }
                return View(cargoOwner);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating cargo owner");
                return View("Error");
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var cargoOwner = await _cargoOwnerService.GetCargoOwnerByIdAsync(id);
                if (cargoOwner == null)
                {
                    return NotFound();
                }
                return View(cargoOwner);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching cargo owner for edit");
                return View("Error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CargoOwnerDto cargoOwner)
        {
            try
            {
                if (id != cargoOwner.Id)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    await _cargoOwnerService.UpdateCargoOwnerAsync(cargoOwner);
                    return RedirectToAction(nameof(Index));
                }
                return View(cargoOwner);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating cargo owner");
                return View("Error");
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var cargoOwner = await _cargoOwnerService.GetCargoOwnerByIdAsync(id);
                if (cargoOwner == null)
                {
                    return NotFound();
                }
                return View(cargoOwner);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching cargo owner for delete");
                return View("Error");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _cargoOwnerService.DeleteCargoOwnerAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting cargo owner");
                return View("Error");
            }
        }

        public async Task<IActionResult> CargoHistory(int id)
        {
            try
            {
                var cargoHistory = await _cargoOwnerService.GetCargoHistoryAsync(id);
                return View(cargoHistory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching cargo history");
                return View("Error");
            }
        }

        public async Task<IActionResult> Verify(int id)
        {
            try
            {
                await _cargoOwnerService.VerifyCargoOwnerAsync(id);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while verifying cargo owner");
                return View("Error");
            }
        }

        public async Task<IActionResult> Block(int id)
        {
            try
            {
                await _cargoOwnerService.BlockCargoOwnerAsync(id);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while blocking cargo owner");
                return View("Error");
            }
        }

        public async Task<IActionResult> Unblock(int id)
        {
            try
            {
                await _cargoOwnerService.UnblockCargoOwnerAsync(id);
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while unblocking cargo owner");
                return View("Error");
            }
        }
    }
} 