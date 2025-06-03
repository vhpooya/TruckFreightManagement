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

namespace TruckFreight.Application.Features.Notifications.Queries.GetNotificationPreferences
{
    public class GetNotificationPreferencesQuery : IRequest<Result<NotificationPreferencesDto>>
    {
        public string UserId { get; set; }
    }

    public class GetNotificationPreferencesQueryValidator : AbstractValidator<GetNotificationPreferencesQuery>
    {
        public GetNotificationPreferencesQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");
        }
    }

    public class GetNotificationPreferencesQueryHandler : IRequestHandler<GetNotificationPreferencesQuery, Result<NotificationPreferencesDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetNotificationPreferencesQueryHandler> _logger;

        public GetNotificationPreferencesQueryHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<GetNotificationPreferencesQueryHandler> logger)
        {
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Result<NotificationPreferencesDto>> Handle(GetNotificationPreferencesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Check if user is authorized to view preferences
                var currentUserId = _currentUserService.UserId;
                if (currentUserId != request.UserId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to view these notification preferences");
                }

                // Get user's notification preferences
                var preferences = await _context.NotificationPreferences
                    .FirstOrDefaultAsync(x => x.UserId == request.UserId, cancellationToken);

                if (preferences == null)
                {
                    // Return default preferences if none exist
                    return Result<NotificationPreferencesDto>.Success(new NotificationPreferencesDto
                    {
                        UserId = request.UserId,
                        EnablePushNotifications = true,
                        EnableEmailNotifications = true,
                        EnableSmsNotifications = true,
                        TypePreferences = new Dictionary<string, NotificationTypePreferences>
                        {
                            { NotificationTypes.TripStatus, new NotificationTypePreferences { EnablePush = true, EnableEmail = true, EnableSms = true, EnableInApp = true } },
                            { NotificationTypes.Payment, new NotificationTypePreferences { EnablePush = true, EnableEmail = true, EnableSms = true, EnableInApp = true } },
                            { NotificationTypes.CargoRequest, new NotificationTypePreferences { EnablePush = true, EnableEmail = true, EnableSms = true, EnableInApp = true } },
                            { NotificationTypes.DriverAssignment, new NotificationTypePreferences { EnablePush = true, EnableEmail = true, EnableSms = true, EnableInApp = true } },
                            { NotificationTypes.DeliveryConfirmation, new NotificationTypePreferences { EnablePush = true, EnableEmail = true, EnableSms = true, EnableInApp = true } },
                            { NotificationTypes.System, new NotificationTypePreferences { EnablePush = true, EnableEmail = true, EnableSms = false, EnableInApp = true } },
                            { NotificationTypes.Rating, new NotificationTypePreferences { EnablePush = true, EnableEmail = true, EnableSms = false, EnableInApp = true } },
                            { NotificationTypes.Violation, new NotificationTypePreferences { EnablePush = true, EnableEmail = true, EnableSms = true, EnableInApp = true } },
                            { NotificationTypes.Maintenance, new NotificationTypePreferences { EnablePush = true, EnableEmail = true, EnableSms = true, EnableInApp = true } },
                            { NotificationTypes.Document, new NotificationTypePreferences { EnablePush = true, EnableEmail = true, EnableSms = false, EnableInApp = true } }
                        },
                        PriorityPreferences = new Dictionary<string, NotificationPriorityPreferences>
                        {
                            { NotificationPriorities.Low, new NotificationPriorityPreferences { EnablePush = true, EnableEmail = true, EnableSms = false, EnableInApp = true, OverrideQuietHours = false } },
                            { NotificationPriorities.Medium, new NotificationPriorityPreferences { EnablePush = true, EnableEmail = true, EnableSms = true, EnableInApp = true, OverrideQuietHours = false } },
                            { NotificationPriorities.High, new NotificationPriorityPreferences { EnablePush = true, EnableEmail = true, EnableSms = true, EnableInApp = true, OverrideQuietHours = true } },
                            { NotificationPriorities.Urgent, new NotificationPriorityPreferences { EnablePush = true, EnableEmail = true, EnableSms = true, EnableInApp = true, OverrideQuietHours = true } }
                        },
                        EnableQuietHours = false,
                        CreatedAt = DateTime.UtcNow
                    });
                }

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
                _logger.LogError(ex, "Error retrieving notification preferences for user {UserId}", request.UserId);
                return Result<NotificationPreferencesDto>.Failure("Failed to retrieve notification preferences");
            }
        }
    }
} 