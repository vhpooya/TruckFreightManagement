using System.Threading.Tasks;
using TruckFreight.Application.Features.Authentication.Models;

namespace TruckFreight.Application.Common.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<bool> PhoneNumberExistsAsync(string phoneNumber);
        Task<bool> SendOtpAsync(string phoneNumber);
        Task<bool> VerifyOtpAsync(string phoneNumber, string otp);
    }
} 