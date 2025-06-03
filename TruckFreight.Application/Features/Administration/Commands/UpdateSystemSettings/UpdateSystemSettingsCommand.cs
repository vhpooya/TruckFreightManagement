using System;
using MediatR;
using FluentValidation;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Administration.Commands.UpdateSystemSettings
{
    public class UpdateSystemSettingsCommand : IRequest<Guid>
    {
        public string SettingKey { get; set; }
        public string SettingValue { get; set; }
        public string Description { get; set; }
    }

    public class UpdateSystemSettingsCommandValidator : AbstractValidator<UpdateSystemSettingsCommand>
    {
        public UpdateSystemSettingsCommandValidator()
        {
            RuleFor(x => x.SettingKey).NotEmpty().MaximumLength(50);
            RuleFor(x => x.SettingValue).NotEmpty().MaximumLength(500);
            RuleFor(x => x.Description).MaximumLength(200);
        }
    }

    public class UpdateSystemSettingsCommandHandler : IRequestHandler<UpdateSystemSettingsCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public UpdateSystemSettingsCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(UpdateSystemSettingsCommand request, CancellationToken cancellationToken)
        {
            var entity = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.SettingKey == request.SettingKey, cancellationToken);

            if (entity == null)
            {
                entity = new SystemSettings(
                    request.SettingKey,
                    request.SettingValue,
                    request.Description
                );
                _context.SystemSettings.Add(entity);
            }
            else
            {
                entity.Update(request.SettingValue, request.Description);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }
    }
} 