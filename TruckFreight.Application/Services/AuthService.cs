// In TruckFreightSystem.Application.Services/AuthService.cs
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TruckFreightSystem.Application.Common.Exceptions; // Assuming you have a folder for custom exceptions
using TruckFreightSystem.Application.DTOs.Auth;
using TruckFreightSystem.Application.Interfaces.Persistence;
using TruckFreightSystem.Application.Interfaces.Services;
using TruckFreightSystem.Domain.Entities;
using TruckFreightSystem.Domain.Enums;
using Microsoft.Extensions.Caching.Memory;
using TruckFreight.Infrastructure.Services.Sms;

namespace TruckFreightSystem.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;
        private readonly SmsService _smsService;
        private readonly IMemoryCache _cache;
        // Consider adding an ISmsService for OTP

        public AuthService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<AuthService> logger, IConfiguration configuration, SmsService smsService, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
            _smsService = smsService;
            _cache = cache;
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(request.PhoneNumber) || string.IsNullOrWhiteSpace(request.Password) || request.Password != request.ConfirmPassword)
            {
                throw new ValidationException("Phone number, password, and confirmation are required and must match.");
            }

            if (!await _unitOfWork.Users.IsPhoneNumberUniqueAsync(request.PhoneNumber))
            {
                throw new DuplicateEntryException($"Phone number {request.PhoneNumber} is already registered.");
            }

            var user = _mapper.Map<User>(request);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password); // Hash password
            user.IsActive = true; // Active by default upon registration
            user.IsVerified = false; // Requires admin verification later

            // Initialize Driver or CargoOwner profile based on role
            if (user.Role == UserRole.Driver)
            {
                user.Driver = new Driver();
            }
            else if (user.Role == UserRole.CargoOwner)
            {
                user.CargoOwner = new CargoOwner();
            }
            else if (user.Role == UserRole.Admin)
            {
                throw new UnauthorizedAccessException("Admin role cannot be registered via public API.");
            }

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("User {PhoneNumber} registered successfully with role {Role}.", user.PhoneNumber, user.Role);

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:AccessTokenExpirationMinutes"])),
                IsVerified = user.IsVerified,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _unitOfWork.Users.GetUserByPhoneNumberAsync(request.PhoneNumber);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid phone number or password.");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("User account is deactivated.");
            }

            // You might want to prevent login for unverified users based on business rules
            // if (!user.IsVerified && user.Role != UserRole.Admin)
            // {
            //     throw new UnauthorizedAccessException("Account not verified by admin.");
            // }

            _logger.LogInformation("User {PhoneNumber} logged in successfully.", user.PhoneNumber);

            // Generate JWT token
            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                UserId = user.Id,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["JwtSettings:AccessTokenExpirationMinutes"])),
                IsVerified = user.IsVerified,
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }

        public async Task<bool> PhoneNumberExistsAsync(string phoneNumber)
        {
            return await _unitOfWork.Users.GetUserByPhoneNumberAsync(phoneNumber) != null;
        }

        public async Task<bool> SendOtpAsync(string phoneNumber)
        {
            var otp = new Random().Next(100000, 999999).ToString();
            _logger.LogInformation("Sending OTP to {PhoneNumber}", phoneNumber);
            var expirationMinutesSetting = _configuration["OtpSettings:ExpirationMinutes"];
            var expirationMinutes = string.IsNullOrEmpty(expirationMinutesSetting) ? 5 : Convert.ToDouble(expirationMinutesSetting);
            _cache.Set(phoneNumber, otp, TimeSpan.FromMinutes(expirationMinutes));
            await _smsService.SendVerificationCodeAsync(phoneNumber, otp);
            return true;
        }

        public async Task<bool> VerifyOtpAsync(string phoneNumber, string otp)
        {
            _logger.LogInformation("Verifying OTP {Otp} for {PhoneNumber}", otp, phoneNumber);
            if (_cache.TryGetValue(phoneNumber, out string cachedOtp) && cachedOtp == otp)
            {
                _cache.Remove(phoneNumber);
                return true;
            }
            return false;
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured."));
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expirationMinutes = Convert.ToDouble(jwtSettings["AccessTokenExpirationMinutes"]);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            if (!string.IsNullOrEmpty(user.FirstName)) claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
            if (!string.IsNullOrEmpty(user.LastName)) claims.Add(new Claim(ClaimTypes.Surname, user.LastName));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}