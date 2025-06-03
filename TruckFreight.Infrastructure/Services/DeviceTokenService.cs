using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;

namespace TruckFreight.Infrastructure.Services
{
    public class DeviceTokenService : IDeviceTokenService
    {
        private readonly ILogger<DeviceTokenService> _logger;
        private readonly IApplicationDbContext _context;

        public DeviceTokenService(
            ILogger<DeviceTokenService> logger,
            IApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<Result> RegisterDeviceTokenAsync(string userId, string deviceToken, string deviceType)
        {
            try
            {
                var existingToken = await _context.DeviceTokens
                    .FirstOrDefaultAsync(x => x.Token == deviceToken);

                if (existingToken != null)
                {
                    if (existingToken.UserId != userId)
                    {
                        existingToken.UserId = userId;
                        existingToken.DeviceType = deviceType;
                        existingToken.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    var newToken = new DeviceToken
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserId = userId,
                        Token = deviceToken,
                        DeviceType = deviceType,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.DeviceTokens.Add(newToken);
                }

                await _context.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering device token for user {UserId}", userId);
                return Result.Failure("Failed to register device token");
            }
        }

        public async Task<Result> UnregisterDeviceTokenAsync(string deviceToken)
        {
            try
            {
                var token = await _context.DeviceTokens
                    .FirstOrDefaultAsync(x => x.Token == deviceToken);

                if (token != null)
                {
                    token.IsActive = false;
                    token.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unregistering device token {DeviceToken}", deviceToken);
                return Result.Failure("Failed to unregister device token");
            }
        }

        public async Task<List<string>> GetUserDeviceTokensAsync(string userId)
        {
            try
            {
                var tokens = await _context.DeviceTokens
                    .Where(x => x.UserId == userId && x.IsActive)
                    .Select(x => x.Token)
                    .ToListAsync();

                return tokens;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting device tokens for user {UserId}", userId);
                return new List<string>();
            }
        }

        public async Task<Result> UpdateDeviceTokenAsync(string oldToken, string newToken)
        {
            try
            {
                var token = await _context.DeviceTokens
                    .FirstOrDefaultAsync(x => x.Token == oldToken);

                if (token != null)
                {
                    token.Token = newToken;
                    token.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating device token from {OldToken} to {NewToken}", oldToken, newToken);
                return Result.Failure("Failed to update device token");
            }
        }
    }

    public class DeviceToken
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Token { get; set; }
        public string DeviceType { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 