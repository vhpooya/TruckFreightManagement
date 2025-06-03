using System;
using System.Threading.Tasks;

namespace TruckFreight.Application.Common.Interfaces
{
    public interface IIdentityService
    {
        Task<bool> IsInRoleAsync(Guid userId, string role);
        Task<bool> AuthorizeAsync(Guid userId, string policy);
    }
} 