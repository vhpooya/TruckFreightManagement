using System;

namespace TruckFreight.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string UserName { get; }
        string Email { get; }
        string Role { get; }
        bool IsAuthenticated { get; }
    }
} 