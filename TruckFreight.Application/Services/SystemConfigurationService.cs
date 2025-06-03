// In TruckFreightSystem.Application.Services/SystemConfigurationService.cs
using AutoMapper;
using Microsoft.Extensions.Logging;
using TruckFreight.Domain.Interfaces;
using TruckFreightSystem.Application.Common.Exceptions;
using TruckFreightSystem.Application.DTOs.SystemConfig;
using TruckFreightSystem.Application.Interfaces.Persistence;
using TruckFreightSystem.Application.Interfaces.Services;
using TruckFreightSystem.Domain.Entities;

namespace TruckFreightSystem.Application.Services
{
    public class SystemConfigurationService : ISystemConfigurationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SystemConfigurationService> _logger;

        public SystemConfigurationService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SystemConfigurationService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<string?> GetConfigValueAsync(string key)
        {
            return await _unitOfWork.SystemConfigurations.GetConfigurationValueAsync(key);
        }

        public async Task<SystemConfigDto?> UpdateConfigAsync(Guid configId, UpdateSystemConfigRequest request)
        {
            var config = await _unitOfWork.SystemConfigurations.GetByIdAsync(configId);
            if (config == null)
            {
                throw new NotFoundException($"System configuration with ID {configId} not found.");
            }

            _mapper.Map(request, config);
            config.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SystemConfigurations.UpdateAsync(config);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("System configuration '{Key}' updated by admin.", config.Key);
            return _mapper.Map<SystemConfigDto>(config);
        }

        public async Task<IEnumerable<SystemConfigDto>> GetAllConfigurationsAsync()
        {
            var configs = await _unitOfWork.SystemConfigurations.GetAllActiveConfigurationsAsync(); // Can modify to GetAllAsync() for admin
            return _mapper.Map<IEnumerable<SystemConfigDto>>(configs);
        }
    }
}