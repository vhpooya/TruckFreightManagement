using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Settings.DTOs;

namespace TruckFreight.Application.Features.Settings.Commands.UpdateSystemSettings
{
    public class UpdateSystemSettingsCommand : IRequest<Result<SystemSettingsDto>>
    {
        public UpdateSystemSettingsDto Settings { get; set; }
    }

    public class UpdateSystemSettingsCommandValidator : AbstractValidator<UpdateSystemSettingsCommand>
    {
        public UpdateSystemSettingsCommandValidator()
        {
            RuleFor(x => x.Settings.Key)
                .NotEmpty().WithMessage("Setting key is required")
                .MaximumLength(100).WithMessage("Setting key must not exceed 100 characters");

            RuleFor(x => x.Settings.Value)
                .NotEmpty().WithMessage("Setting value is required")
                .MaximumLength(4000).WithMessage("Setting value must not exceed 4000 characters");

            RuleFor(x => x.Settings.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters");

            RuleFor(x => x.Settings.Category)
                .NotEmpty().WithMessage("Category is required")
                .Must(x => Enum.IsDefined(typeof(SettingCategories), x))
                .WithMessage("Invalid category");

            RuleFor(x => x.Settings.DataType)
                .NotEmpty().WithMessage("Data type is required")
                .Must(x => Enum.IsDefined(typeof(SettingDataTypes), x))
                .WithMessage("Invalid data type");

            RuleFor(x => x.Settings.ValidationRules)
                .MaximumLength(1000).WithMessage("Validation rules must not exceed 1000 characters");

            RuleFor(x => x.Settings.DefaultValue)
                .MaximumLength(4000).WithMessage("Default value must not exceed 4000 characters");
        }
    }

    public class UpdateSystemSettingsCommandHandler : IRequestHandler<UpdateSystemSettingsCommand, Result<SystemSettingsDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateSystemSettingsCommandHandler> _logger;
        private readonly IEncryptionService _encryptionService;

        public UpdateSystemSettingsCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<UpdateSystemSettingsCommandHandler> logger,
            IEncryptionService encryptionService)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
            _encryptionService = encryptionService;
        }

        public async Task<Result<SystemSettingsDto>> Handle(UpdateSystemSettingsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (string.IsNullOrEmpty(userId))
                {
                    return Result<SystemSettingsDto>.Failure("User not authenticated");
                }

                // Get user and check permissions
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

                if (user == null)
                {
                    return Result<SystemSettingsDto>.Failure("User not found");
                }

                if (!user.Roles.Contains("Admin"))
                {
                    return Result<SystemSettingsDto>.Failure("User does not have permission to update system settings");
                }

                // Get existing setting
                var setting = await _context.SystemSettings
                    .FirstOrDefaultAsync(s => s.Key == request.Settings.Key, cancellationToken);

                if (setting == null)
                {
                    // Create new setting
                    setting = new Domain.Entities.SystemSettings
                    {
                        Id = Guid.NewGuid().ToString(),
                        Key = request.Settings.Key,
                        Value = request.Settings.IsEncrypted
                            ? _encryptionService.Encrypt(request.Settings.Value)
                            : request.Settings.Value,
                        Description = request.Settings.Description,
                        Category = request.Settings.Category,
                        DataType = request.Settings.DataType,
                        IsEncrypted = request.Settings.IsEncrypted,
                        IsReadOnly = request.Settings.IsReadOnly,
                        ValidationRules = request.Settings.ValidationRules,
                        DefaultValue = request.Settings.DefaultValue,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userId
                    };

                    _context.SystemSettings.Add(setting);
                }
                else
                {
                    // Check if setting is read-only
                    if (setting.IsReadOnly)
                    {
                        return Result<SystemSettingsDto>.Failure("Cannot update read-only setting");
                    }

                    // Update existing setting
                    setting.Value = request.Settings.IsEncrypted
                        ? _encryptionService.Encrypt(request.Settings.Value)
                        : request.Settings.Value;
                    setting.Description = request.Settings.Description;
                    setting.Category = request.Settings.Category;
                    setting.DataType = request.Settings.DataType;
                    setting.IsEncrypted = request.Settings.IsEncrypted;
                    setting.IsReadOnly = request.Settings.IsReadOnly;
                    setting.ValidationRules = request.Settings.ValidationRules;
                    setting.DefaultValue = request.Settings.DefaultValue;
                    setting.UpdatedAt = DateTime.UtcNow;
                    setting.UpdatedBy = userId;
                }

                await _context.SaveChangesAsync(cancellationToken);

                // Map to DTO
                var settingDto = new SystemSettingsDto
                {
                    Id = setting.Id,
                    Key = setting.Key,
                    Value = setting.IsEncrypted
                        ? _encryptionService.Decrypt(setting.Value)
                        : setting.Value,
                    Description = setting.Description,
                    Category = setting.Category,
                    DataType = setting.DataType,
                    IsEncrypted = setting.IsEncrypted,
                    IsReadOnly = setting.IsReadOnly,
                    ValidationRules = setting.ValidationRules,
                    DefaultValue = setting.DefaultValue,
                    CreatedAt = setting.CreatedAt,
                    CreatedBy = setting.CreatedBy,
                    UpdatedAt = setting.UpdatedAt,
                    UpdatedBy = setting.UpdatedBy
                };

                return Result<SystemSettingsDto>.Success(settingDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating system settings");
                return Result<SystemSettingsDto>.Failure("Error updating system settings");
            }
        }
    }
} 