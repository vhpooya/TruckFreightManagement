using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Features.CargoOwners.Commands;
using TruckFreight.Application.Features.CargoOwners.Dtos;
using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Interfaces;
using TruckFreight.Infrastructure.Services;

namespace TruckFreight.Application.Services
{
    public class CargoOwnerApplicationService : ICargoOwnerApplicationService
    {
        private readonly ICargoOwnerRepository _cargoOwnerRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<CargoOwnerApplicationService> _logger;

        public CargoOwnerApplicationService(
            ICargoOwnerRepository cargoOwnerRepository,
            IFileStorageService fileStorageService,
            ILogger<CargoOwnerApplicationService> logger)
        {
            _cargoOwnerRepository = cargoOwnerRepository;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<CargoOwnerDto> CreateCargoOwnerAsync(CreateCargoOwnerCommand command)
        {
            try
            {
                var cargoOwner = new CargoOwner
                {
                    UserId = command.UserId,
                    NationalId = command.NationalId,
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    PhoneNumber = command.PhoneNumber,
                    Email = command.Email,
                    Address = command.Address,
                    IsCompany = command.IsCompany,
                    CompanyName = command.CompanyName,
                    CompanyRegistrationNumber = command.CompanyRegistrationNumber,
                    Status = CargoOwnerStatus.Pending
                };

                await _cargoOwnerRepository.AddAsync(cargoOwner);
                return MapToDto(cargoOwner);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating cargo owner");
                throw;
            }
        }

        public async Task<CargoOwnerDto> GetCargoOwnerByIdAsync(int id)
        {
            var cargoOwner = await _cargoOwnerRepository.GetByIdAsync(id);
            return cargoOwner != null ? MapToDto(cargoOwner) : null;
        }

        public async Task<IEnumerable<CargoOwnerDto>> GetCargoOwnersAsync()
        {
            var cargoOwners = await _cargoOwnerRepository.GetAllAsync();
            return cargoOwners.Select(MapToDto);
        }

        public async Task<CargoOwnerDto> UpdateCargoOwnerAsync(UpdateCargoOwnerCommand command)
        {
            var cargoOwner = await _cargoOwnerRepository.GetByIdAsync(command.Id);
            if (cargoOwner == null)
                throw new KeyNotFoundException($"Cargo owner with ID {command.Id} not found");

            cargoOwner.FirstName = command.FirstName;
            cargoOwner.LastName = command.LastName;
            cargoOwner.PhoneNumber = command.PhoneNumber;
            cargoOwner.Email = command.Email;
            cargoOwner.Address = command.Address;

            if (cargoOwner.IsCompany)
            {
                cargoOwner.CompanyName = command.CompanyName;
                cargoOwner.CompanyRegistrationNumber = command.CompanyRegistrationNumber;
            }

            await _cargoOwnerRepository.UpdateAsync(cargoOwner);
            return MapToDto(cargoOwner);
        }

        public async Task DeleteCargoOwnerAsync(int id)
        {
            var cargoOwner = await _cargoOwnerRepository.GetByIdAsync(id);
            if (cargoOwner == null)
                throw new KeyNotFoundException($"Cargo owner with ID {id} not found");

            await _cargoOwnerRepository.DeleteAsync(cargoOwner);
        }

        public async Task<CargoOwnerDto> VerifyCargoOwnerAsync(int id)
        {
            var cargoOwner = await _cargoOwnerRepository.GetByIdAsync(id);
            if (cargoOwner == null)
                throw new KeyNotFoundException($"Cargo owner with ID {id} not found");

            cargoOwner.Status = CargoOwnerStatus.Verified;
            await _cargoOwnerRepository.UpdateAsync(cargoOwner);
            return MapToDto(cargoOwner);
        }

        public async Task<CargoOwnerDto> UploadDocumentsAsync(UploadCargoOwnerDocumentsCommand command)
        {
            var cargoOwner = await _cargoOwnerRepository.GetByIdAsync(command.CargoOwnerId);
            if (cargoOwner == null)
                throw new KeyNotFoundException($"Cargo owner with ID {command.CargoOwnerId} not found");

            // Upload national ID image
            if (command.NationalIdImage != null)
            {
                cargoOwner.NationalIdImageUrl = await _fileStorageService.UploadFileAsync(
                    command.NationalIdImage,
                    $"cargo-owners/{cargoOwner.Id}/documents/national-id");
            }

            // Upload company registration document if applicable
            if (cargoOwner.IsCompany && command.CompanyRegistrationDocument != null)
            {
                cargoOwner.CompanyRegistrationDocumentUrl = await _fileStorageService.UploadFileAsync(
                    command.CompanyRegistrationDocument,
                    $"cargo-owners/{cargoOwner.Id}/documents/company-registration");
            }

            await _cargoOwnerRepository.UpdateAsync(cargoOwner);
            return MapToDto(cargoOwner);
        }

        public async Task<CargoOwnerDto> UpdateProfileAsync(UpdateCargoOwnerProfileCommand command)
        {
            var cargoOwner = await _cargoOwnerRepository.GetByIdAsync(command.CargoOwnerId);
            if (cargoOwner == null)
                throw new KeyNotFoundException($"Cargo owner with ID {command.CargoOwnerId} not found");

            cargoOwner.FirstName = command.FirstName;
            cargoOwner.LastName = command.LastName;
            cargoOwner.PhoneNumber = command.PhoneNumber;
            cargoOwner.Email = command.Email;
            cargoOwner.Address = command.Address;

            if (command.ProfileImage != null)
            {
                cargoOwner.ProfileImageUrl = await _fileStorageService.UploadFileAsync(
                    command.ProfileImage,
                    $"cargo-owners/{cargoOwner.Id}/profile");
            }

            await _cargoOwnerRepository.UpdateAsync(cargoOwner);
            return MapToDto(cargoOwner);
        }

        public async Task<CargoOwnerDto> UpdateCompanyInfoAsync(UpdateCompanyInfoCommand command)
        {
            var cargoOwner = await _cargoOwnerRepository.GetByIdAsync(command.CargoOwnerId);
            if (cargoOwner == null)
                throw new KeyNotFoundException($"Cargo owner with ID {command.CargoOwnerId} not found");

            if (!cargoOwner.IsCompany)
                throw new InvalidOperationException("This cargo owner is not a company");

            cargoOwner.CompanyName = command.CompanyName;
            cargoOwner.CompanyRegistrationNumber = command.CompanyRegistrationNumber;
            cargoOwner.CompanyAddress = command.CompanyAddress;
            cargoOwner.CompanyPhoneNumber = command.CompanyPhoneNumber;
            cargoOwner.CompanyEmail = command.CompanyEmail;

            if (command.CompanyLogo != null)
            {
                cargoOwner.CompanyLogoUrl = await _fileStorageService.UploadFileAsync(
                    command.CompanyLogo,
                    $"cargo-owners/{cargoOwner.Id}/company/logo");
            }

            await _cargoOwnerRepository.UpdateAsync(cargoOwner);
            return MapToDto(cargoOwner);
        }

        private static CargoOwnerDto MapToDto(CargoOwner cargoOwner)
        {
            return new CargoOwnerDto
            {
                Id = cargoOwner.Id,
                UserId = cargoOwner.UserId,
                NationalId = cargoOwner.NationalId,
                FirstName = cargoOwner.FirstName,
                LastName = cargoOwner.LastName,
                PhoneNumber = cargoOwner.PhoneNumber,
                Email = cargoOwner.Email,
                Address = cargoOwner.Address,
                IsCompany = cargoOwner.IsCompany,
                CompanyName = cargoOwner.CompanyName,
                CompanyRegistrationNumber = cargoOwner.CompanyRegistrationNumber,
                Status = cargoOwner.Status,
                ProfileImageUrl = cargoOwner.ProfileImageUrl,
                NationalIdImageUrl = cargoOwner.NationalIdImageUrl,
                CompanyLogoUrl = cargoOwner.CompanyLogoUrl,
                CompanyRegistrationDocumentUrl = cargoOwner.CompanyRegistrationDocumentUrl,
                CreatedAt = cargoOwner.CreatedAt,
                UpdatedAt = cargoOwner.UpdatedAt
            };
        }
    }
} 