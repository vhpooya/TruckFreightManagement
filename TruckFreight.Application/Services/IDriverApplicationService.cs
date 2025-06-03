using System.Collections.Generic;
using System.Threading.Tasks;
using TruckFreight.Application.Features.Drivers.Commands;
using TruckFreight.Application.Features.Drivers.Dtos;

namespace TruckFreight.Application.Services
{
    public interface IDriverApplicationService
    {
        Task<DriverDto> CreateDriverAsync(CreateDriverCommand command);
        Task<DriverDto> GetDriverByIdAsync(int id);
        Task<IEnumerable<DriverDto>> GetDriversAsync();
        Task<DriverDto> UpdateDriverAsync(UpdateDriverCommand command);
        Task DeleteDriverAsync(int id);
        Task<DriverDto> VerifyDriverAsync(int id);
        Task<DriverDto> UploadDocumentsAsync(UploadDriverDocumentsCommand command);
        Task<DriverDto> UpdateProfileAsync(UpdateDriverProfileCommand command);
        Task<DriverDto> UpdateVehicleInfoAsync(UpdateVehicleInfoCommand command);
        Task<DriverDto> UpdateLocationAsync(UpdateDriverLocationCommand command);
        Task<DriverDto> UpdateStatusAsync(UpdateDriverStatusCommand command);
        Task<IEnumerable<CargoRequestDto>> GetNearbyCargoRequestsAsync(int driverId, double latitude, double longitude, double radius);
    }
} 