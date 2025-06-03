using System.Security.Claims;
using TruckFreight.Application.Common.Interfaces;

namespace TruckFreight.WebAPI.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid? UserId 
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue("userId");
                return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
            }
        }

        public string UserName => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name) ?? string.Empty;

        public string Role => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

        public bool IsInRole(string role)
        {
            return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
        }

        public List<string> GetUserRoles()
        {
            return _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role)
                .Select(c => c.Value).ToList() ?? new List<string>();
        }
    }
}

/