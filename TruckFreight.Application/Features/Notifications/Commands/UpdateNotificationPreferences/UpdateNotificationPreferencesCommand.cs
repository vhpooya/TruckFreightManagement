using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Notifications.DTOs;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Notifications.Commands.UpdateNotificationPreferences
{
    public class UpdateNotificationPreferencesCommand : IRequest<Result<NotificationPreferencesDto>>
    {
        public string UserId { get; set; }
        public UpdateNotificationPreferencesDto Preferences { get; set; }
    }

    public class UpdateNotificationPreferencesCommandValidator : AbstractValidator<UpdateNotificationPreferencesCommand>
    {
        public UpdateNotificationPreferencesCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.Preferences)
                .NotNull().WithMessage("Preferences are required");

            When(x => x.Preferences.EnableQuietHours, () =>
            {
                RuleFor(x => x.Preferences.QuietHoursStart)
                    .NotNull().WithMessage("Quiet hours start time is required when quiet hours are enabled");

                RuleFor(x => x.Preferences.QuietHoursEnd)
                    .NotNull().WithMessage("Quiet hours end time is required when quiet hours are enabled");

                RuleFor(x => x.Preferences.QuietHoursStart)
                    .LessThan(x => x.Preferences.QuietHoursEnd)
                    .WithMessage("Quiet hours start time must be before end time");
            });

            RuleFor(x => x.Preferences.TypePreferences)
                .NotNull().WithMessage("Type preferences are required");

            RuleFor(x => x.Preferences.PriorityPreferences)
                .NotNull().WithMessage("Priority preferences are required");
        }
    }

    public class UpdateNotificationPreferencesCommandHandler : IRequestHandler<UpdateNotificationPreferencesCommand, Result<NotificationPreferencesDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateNotificationPreferencesCommandHandler> _logger;

        public UpdateNotificationPreferencesCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<UpdateNotificationPreferencesCommandHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<NotificationPreferencesDto>> Handle(UpdateNotificationPreferencesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if user is authorized to update preferences
                var currentUserId = _currentUserService.UserId;
                if (currentUserId != request.UserId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to update these notification preferences");
                }

                // Get user's notification preferences
                var preferences = await _context.NotificationPreferences
                    .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

                if (preferences == null)
                {
                    // Create new preferences if they don't exist
                    preferences = new NotificationPreferences
                    {
                        UserId = request.UserId,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.NotificationPreferences.Add(preferences);
                }

                // Update preferences
                preferences.EnablePushNotifications = request.Preferences.EnablePushNotifications;
                preferences.EnableEmailNotifications = request.Preferences.EnableEmailNotifications;
                preferences.EnableSmsNotifications = request.Preferences.EnableSmsNotifications;
                preferences.TypePreferences = request.Preferences.TypePreferences;
                preferences.PriorityPreferences = request.Preferences.PriorityPreferences;
                preferences.QuietHoursStart = request.Preferences.QuietHoursStart;
                preferences.QuietHoursEnd = request.Preferences.QuietHoursEnd;
                preferences.EnableQuietHours = request.Preferences.EnableQuietHours;
                preferences.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);

                // Map to DTO
                var preferencesDto = new NotificationPreferencesDto
                {
                    UserId = preferences.UserId,
                    EnablePushNotifications = preferences.EnablePushNotifications,
                    EnableEmailNotifications = preferences.EnableEmailNotifications,
                    EnableSmsNotifications = preferences.EnableSmsNotifications,
                    TypePreferences = preferences.TypePreferences,
                    PriorityPreferences = preferences.PriorityPreferences,
                    QuietHoursStart = preferences.QuietHoursStart,
                    QuietHoursEnd = preferences.QuietHoursEnd,
                    EnableQuietHours = preferences.EnableQuietHours,
                    CreatedAt = preferences.CreatedAt,
                    UpdatedAt = preferences.UpdatedAt
                };

                return Result<NotificationPreferencesDto>.Success(preferencesDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification preferences for user {UserId}", request.UserId);
                return Result<NotificationPreferencesDto>.Failure("Failed to update notification preferences");
            }
        }
    }
} 