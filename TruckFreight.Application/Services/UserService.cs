// In TruckFreightSystem.Application.Services/UserService.cs
using AutoMapper;
using Microsoft.Extensions.Logging;
using TruckFreight.Domain.Enums;
using TruckFreight.Domain.Interfaces;


namespace TruckFreightSystem.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", userId);
                throw new NotFoundException($"User with ID {userId} not found.");
            }
            return _mapper.Map<UserProfileDto>(user);
        }

        public async Task<UserProfileDto?> UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for update.", userId);
                throw new NotFoundException($"User with ID {userId} not found.");
            }

            _mapper.Map(request, user); // Apply updates from DTO to entity
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("User {UserId} profile updated.", userId);
            return _mapper.Map<UserProfileDto>(user);
        }

        public async Task<DriverProfileDto?> GetDriverProfileAsync(Guid userId)
        {
            var driver = await _unitOfWork.Drivers.GetDriverByUserIdAsync(userId);
            if (driver == null)
            {
                _logger.LogWarning("Driver profile for User ID {UserId} not found.", userId);
                throw new NotFoundException($"Driver profile for User ID {userId} not found.");
            }
            await _unitOfWork.Drivers.LoadReferenceAsync(driver, d => d.User); // Load related User
            await _unitOfWork.Drivers.LoadCollectionAsync(driver, d => d.Trucks); // Load related Trucks
            return _mapper.Map<DriverProfileDto>(driver);
        }

        public async Task<DriverProfileDto?> UpdateDriverProfileAsync(Guid userId, UpdateDriverProfileRequest request)
        {
            var driver = await _unitOfWork.Drivers.GetDriverByUserIdAsync(userId);
            if (driver == null)
            {
                _logger.LogWarning("Driver profile for User ID {UserId} not found for update.", userId);
                throw new NotFoundException($"Driver profile for User ID {userId} not found.");
            }

            // Update User properties from the request
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException($"Associated User for Driver ID {userId} not found.");
            }
            _mapper.Map(request, user); // Map common fields to User
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);


            _mapper.Map(request, driver); // Apply updates from DTO to driver entity
            driver.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Drivers.UpdateAsync(driver);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Driver profile for User {UserId} updated.", userId);
            await _unitOfWork.Drivers.LoadReferenceAsync(driver, d => d.User); // Reload user for DTO mapping
            await _unitOfWork.Drivers.LoadCollectionAsync(driver, d => d.Trucks); // Reload trucks for DTO mapping
            return _mapper.Map<DriverProfileDto>(driver);
        }

        public async Task<CargoOwnerProfileDto?> GetCargoOwnerProfileAsync(Guid userId)
        {
            var cargoOwner = await _unitOfWork.CargoOwners.GetCargoOwnerByUserIdAsync(userId);
            if (cargoOwner == null)
            {
                _logger.LogWarning("CargoOwner profile for User ID {UserId} not found.", userId);
                throw new NotFoundException($"CargoOwner profile for User ID {userId} not found.");
            }
            await _unitOfWork.CargoOwners.LoadReferenceAsync(cargoOwner, co => co.User); // Load related User
            return _mapper.Map<CargoOwnerProfileDto>(cargoOwner);
        }

        public async Task<CargoOwnerProfileDto?> UpdateCargoOwnerProfileAsync(Guid userId, UpdateCargoOwnerProfileRequest request)
        {
            var cargoOwner = await _unitOfWork.CargoOwners.GetCargoOwnerByUserIdAsync(userId);
            if (cargoOwner == null)
            {
                _logger.LogWarning("CargoOwner profile for User ID {UserId} not found for update.", userId);
                throw new NotFoundException($"CargoOwner profile for User ID {userId} not found.");
            }

            // Update User properties from the request
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException($"Associated User for CargoOwner ID {userId} not found.");
            }
            _mapper.Map(request, user); // Map common fields to User
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);

            _mapper.Map(request, cargoOwner); // Apply updates from DTO to cargo owner entity
            cargoOwner.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CargoOwners.UpdateAsync(cargoOwner);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("CargoOwner profile for User {UserId} updated.", userId);
            await _unitOfWork.CargoOwners.LoadReferenceAsync(cargoOwner, co => co.User); // Reload user for DTO mapping
            return _mapper.Map<CargoOwnerProfileDto>(cargoOwner);
        }

        public async Task<bool> DeactivateUserAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for deactivation.", userId);
                return false;
            }

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("User {UserId} deactivated.", userId);
            return true;
        }

        public async Task<bool> ActivateUserAsync(Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for activation.", userId);
                return false;
            }

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("User {UserId} activated.", userId);
            return true;
        }

        public async Task<bool> VerifyUserAsync(Guid userId, bool isVerified)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found for verification.", userId);
                return false;
            }

            user.IsVerified = isVerified;
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.CompleteAsync();
            _logger.LogInformation("User {UserId} verification status set to {IsVerified}.", userId, isVerified);
            return true;
        }

        public async Task<IEnumerable<UserProfileDto>> GetAllUsersAsync(UserRoleType? role = null, bool? isActive = null, bool? isVerified = null)
        {
            var users = await _unitOfWork.Users.GetAllAsync(); // This might need to be optimized for large datasets (paging)

            if (role.HasValue)
            {
                users = users.Where(u => u.Role == role.Value).ToList();
            }
            if (isActive.HasValue)
            {
                users = users.Where(u => u.IsActive == isActive.Value).ToList();
            }
            if (isVerified.HasValue)
            {
                users = users.Where(u => u.IsVerified == isVerified.Value).ToList();
            }

            return _mapper.Map<IEnumerable<UserProfileDto>>(users);
        }
    }
}