using System.Collections.Generic;
using System.Threading.Tasks;
using TruckFreight.Application.Features.CargoOwners.Commands;
using TruckFreight.Application.Features.CargoOwners.Dtos;

namespace TruckFreight.Application.Services
{
    public interface ICargoOwnerApplicationService
    {
        Task<CargoOwnerDto> CreateCargoOwnerAsync(CreateCargoOwnerCommand command);
        Task<CargoOwnerDto> GetCargoOwnerByIdAsync(int id);
        Task<IEnumerable<CargoOwnerDto>> GetCargoOwnersAsync();
        Task<CargoOwnerDto> UpdateCargoOwnerAsync(UpdateCargoOwnerCommand command);
        Task DeleteCargoOwnerAsync(int id);
        Task<CargoOwnerDto> VerifyCargoOwnerAsync(int id);
        Task<CargoOwnerDto> UploadDocumentsAsync(UploadCargoOwnerDocumentsCommand command);
        Task<CargoOwnerDto> UpdateProfileAsync(UpdateCargoOwnerProfileCommand command);
        Task<CargoOwnerDto> UpdateCompanyInfoAsync(UpdateCompanyInfoCommand command);
    }
} 