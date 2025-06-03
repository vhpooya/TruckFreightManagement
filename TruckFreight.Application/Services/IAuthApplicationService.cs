using System.Threading.Tasks;
using TruckFreight.Application.Features.Auth.Commands;
using TruckFreight.Application.Features.Auth.Dtos;

namespace TruckFreight.Application.Services
{
    public interface IAuthApplicationService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterCommand command);
        Task<AuthResponseDto> LoginAsync(LoginCommand command);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenCommand command);
        Task ForgotPasswordAsync(ForgotPasswordCommand command);
        Task ResetPasswordAsync(ResetPasswordCommand command);
        Task ChangePasswordAsync(ChangePasswordCommand command);
        Task VerifyEmailAsync(VerifyEmailCommand command);
        Task ResendVerificationEmailAsync(ResendVerificationEmailCommand command);
        Task<UserDto> GetCurrentUserAsync();
        Task LogoutAsync();
    }
} 