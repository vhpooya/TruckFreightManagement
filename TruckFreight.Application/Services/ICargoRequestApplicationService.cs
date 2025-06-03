using TruckFreight.Application.Features.CargoRequests.Commands;
using TruckFreight.Application.Features.CargoRequests.Queries;
using TruckFreight.Application.Models.Dtos;

namespace TruckFreight.Application.Services
{
    public interface ICargoRequestApplicationService
    {
        Task<CargoRequestDto> CreateCargoRequestAsync(CreateCargoRequestCommand command);
        Task<CargoRequestDto> GetCargoRequestByIdAsync(GetCargoRequestDetailsQuery query);
        Task<IEnumerable<CargoRequestDto>> GetCargoRequestsAsync(GetCargoRequestsQuery query);
        Task<IEnumerable<CargoRequestDto>> GetNearbyCargoRequestsAsync(GetNearbyCargoRequestsQuery query);
        Task<IEnumerable<CargoRequestDto>> GetMyCargoRequestsAsync(GetMyCargoRequestsQuery query);
        Task<CargoRequestDto> UpdateCargoRequestAsync(UpdateCargoRequestCommand command);
        Task<bool> CancelCargoRequestAsync(CancelCargoRequestCommand command);
        Task<bool> PublishCargoRequestAsync(PublishCargoRequestCommand command);
        Task<bool> AssignDriverAsync(AssignDriverCommand command);
        Task<bool> UpdateStatusAsync(UpdateCargoRequestStatusCommand command);
        Task<bool> UpdateLocationAsync(UpdateCargoRequestLocationCommand command);
        Task<string> UploadCargoImagesAsync(UploadCargoImagesCommand command);
    }
} 