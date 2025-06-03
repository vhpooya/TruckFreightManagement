using Microsoft.EntityFrameworkCore;
using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Enums;
using TruckFreight.Domain.Interfaces;
using TruckFreight.Domain.ValueObjects;
using TruckFreight.Persistence.Context;

namespace TruckFreight.Persistence.Repositories
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(TruckFreightDbContext context) : base(context)
        {
        }

        public async Task<User> GetByNationalIdAsync(string nationalId, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.NationalId == nationalId, cancellationToken);
        }

        public async Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.Email == email, cancellationToken);
        }

        public async Task<User> GetByPhoneNumberAsync(PhoneNumber phoneNumber, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(x => x.PhoneNumber.Number == phoneNumber.Number, cancellationToken);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(x => x.Role == role).ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<User>> GetUsersByStatusAsync(UserStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(x => x.Status == status).ToListAsync(cancellationToken);
        }

        public async Task<bool> IsNationalIdExistsAsync(string nationalId, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(x => x.NationalId == nationalId);
            
            if (excludeUserId.HasValue)
            {
                query = query.Where(x => x.Id != excludeUserId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> IsEmailExistsAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var query = _dbSet.Where(x => x.Email == email);
            
            if (excludeUserId.HasValue)
            {
                query = query.Where(x => x.Id != excludeUserId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> IsPhoneNumberExistsAsync(PhoneNumber phoneNumber, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Where(x => x.PhoneNumber.Number == phoneNumber.Number);
            
            if (excludeUserId.HasValue)
            {
                query = query.Where(x => x.Id != excludeUserId.Value);
            }

            return await query.AnyAsync(cancellationToken);
        }

        public async Task<(IEnumerable<User> Users, int TotalCount)> SearchUsersAsync(
            string searchTerm, UserRole? role, UserStatus? status,
            int pageNumber, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim().ToLower();
                query = query.Where(x => 
                    x.FirstName.ToLower().Contains(searchTerm) ||
                    x.LastName.ToLower().Contains(searchTerm) ||
                    x.NationalId.Contains(searchTerm) ||
                    x.Email.ToLower().Contains(searchTerm) ||
                    x.PhoneNumber.Number.Contains(searchTerm));
            }

            if (role.HasValue)
            {
                query = query.Where(x => x.Role == role.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(x => x.Status == status.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var users = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (users, totalCount);
        }
    }
}

// TruckFreight.Domain/Interfaces/IDriverRepository.cs