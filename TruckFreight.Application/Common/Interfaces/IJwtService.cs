using System.Threading.Tasks;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Common.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        DateTime GetExpiry();
        Task<bool> ValidateTokenAsync(string token);
        Task<string> RefreshTokenAsync(string token);
    }
} 