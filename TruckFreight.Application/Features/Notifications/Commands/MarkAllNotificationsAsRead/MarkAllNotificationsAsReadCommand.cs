using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Notifications.Commands.MarkAllNotificationsAsRead
{
    public class MarkAllNotificationsAsReadCommand : IRequest<Result>
    {
    }

    public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, Result>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public MarkAllNotificationsAsReadCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
        {
            var notifications = await _context.Notifications
                .Where(x => x.UserId == _currentUserService.UserId && !x.IsRead)
                .ToListAsync(cancellationToken);

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success("All notifications marked as read");
        }
    }
} 