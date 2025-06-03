using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using TruckFreight.Application.Features.Auth.Commands;
using TruckFreight.Application.Features.Auth.Dtos;
using TruckFreight.Application.Services;
using TruckFreight.Domain.Entities;

namespace TruckFreight.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthApplicationService _authService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthApplicationService authService,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterCommand command)
        {
            try
            {
                var result = await _authService.RegisterAsync(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while registering user");
                return StatusCode(500, "An error occurred while registering the user");
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginCommand command)
        {
            try
            {
                var result = await _authService.LoginAsync(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while logging in");
                return StatusCode(500, "An error occurred while logging in");
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            try
            {
                var result = await _authService.RefreshTokenAsync(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while refreshing token");
                return StatusCode(500, "An error occurred while refreshing token");
            }
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
        {
            try
            {
                await _authService.ForgotPasswordAsync(command);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing forgot password request");
                return StatusCode(500, "An error occurred while processing forgot password request");
            }
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            try
            {
                await _authService.ResetPasswordAsync(command);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while resetting password");
                return StatusCode(500, "An error occurred while resetting password");
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            try
            {
                await _authService.ChangePasswordAsync(command);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while changing password");
                return StatusCode(500, "An error occurred while changing password");
            }
        }

        [HttpPost("verify-email")]
        public async Task<ActionResult> VerifyEmail([FromBody] VerifyEmailCommand command)
        {
            try
            {
                await _authService.VerifyEmailAsync(command);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while verifying email");
                return StatusCode(500, "An error occurred while verifying email");
            }
        }

        [HttpPost("resend-verification-email")]
        public async Task<ActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailCommand command)
        {
            try
            {
                await _authService.ResendVerificationEmailAsync(command);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while resending verification email");
                return StatusCode(500, "An error occurred while resending verification email");
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            try
            {
                var result = await _authService.GetCurrentUserAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting current user");
                return StatusCode(500, "An error occurred while getting current user");
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout()
        {
            try
            {
                await _authService.LogoutAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while logging out");
                return StatusCode(500, "An error occurred while logging out");
            }
        }
    }
}

/