using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TruckFreight.Application.Features.Auth.Commands;
using TruckFreight.Application.Features.Auth.Dtos;
using TruckFreight.Domain.Entities;
using TruckFreight.Domain.Interfaces;

namespace TruckFreight.Application.Services
{
    public class AuthApplicationService : IAuthApplicationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthApplicationService> _logger;

        public AuthApplicationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration,
            IEmailService emailService,
            ILogger<AuthApplicationService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterCommand command)
        {
            var user = new ApplicationUser
            {
                UserName = command.Email,
                Email = command.Email,
                PhoneNumber = command.PhoneNumber,
                FirstName = command.FirstName,
                LastName = command.LastName
            };

            var result = await _userManager.CreateAsync(user, command.Password);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            await _userManager.AddToRoleAsync(user, command.Role);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _emailService.SendEmailConfirmationAsync(user.Email, token);

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Token = GenerateJwtToken(user),
                RefreshToken = GenerateRefreshToken()
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginCommand command)
        {
            var user = await _userManager.FindByEmailAsync(command.Email);
            if (user == null)
            {
                throw new Exception("Invalid email or password");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, command.Password, false);
            if (!result.Succeeded)
            {
                throw new Exception("Invalid email or password");
            }

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Token = GenerateJwtToken(user),
                RefreshToken = GenerateRefreshToken()
            };
        }

        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenCommand command)
        {
            var principal = GetPrincipalFromExpiredToken(command.Token);
            var user = await _userManager.FindByIdAsync(principal.FindFirstValue(ClaimTypes.NameIdentifier));

            if (user == null || user.RefreshToken != command.RefreshToken)
            {
                throw new Exception("Invalid refresh token");
            }

            return new AuthResponseDto
            {
                UserId = user.Id,
                Email = user.Email,
                Token = GenerateJwtToken(user),
                RefreshToken = GenerateRefreshToken()
            };
        }

        public async Task ForgotPasswordAsync(ForgotPasswordCommand command)
        {
            var user = await _userManager.FindByEmailAsync(command.Email);
            if (user == null)
            {
                return;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailService.SendPasswordResetAsync(user.Email, token);
        }

        public async Task ResetPasswordAsync(ResetPasswordCommand command)
        {
            var user = await _userManager.FindByEmailAsync(command.Email);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var result = await _userManager.ResetPasswordAsync(user, command.Token, command.NewPassword);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        public async Task ChangePasswordAsync(ChangePasswordCommand command)
        {
            var user = await _userManager.FindByIdAsync(command.UserId);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var result = await _userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        public async Task VerifyEmailAsync(VerifyEmailCommand command)
        {
            var user = await _userManager.FindByEmailAsync(command.Email);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var result = await _userManager.ConfirmEmailAsync(user, command.Token);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        public async Task ResendVerificationEmailAsync(ResendVerificationEmailCommand command)
        {
            var user = await _userManager.FindByEmailAsync(command.Email);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _emailService.SendEmailConfirmationAsync(user.Email, token);
        }

        public async Task<UserDto> GetCurrentUserAsync()
        {
            var user = await _userManager.GetUserAsync(ClaimsPrincipal.Current);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = await _userManager.GetRolesAsync(user)
            };
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                ValidateLifetime = false,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (!(securityToken is JwtSecurityToken jwtSecurityToken) ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
} 