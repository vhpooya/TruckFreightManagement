using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Enums;
using TruckFreight.Domain.ValueObjects;

namespace TruckFreight.Domain.Interfaces
{
    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken = default);
        Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User> GetByPhoneNumberAsync(PhoneNumber phoneNumber, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetUsersByStatusAsync(UserStatus status, CancellationToken cancellationToken = default);
        Task<bool> IsNationalIdExistsAsync(string nationalId, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
        Task<bool> IsEmailExistsAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
        Task<bool> IsPhoneNumberExistsAsync(PhoneNumber phoneNumber, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
        Task<(IEnumerable<User> Users, int TotalCount)> SearchUsersAsync(
            string searchTerm, UserRole? role, UserStatus? status, 
            int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    }
}

// TruckFreight.Persistence/Repositories/UserRepository.cs