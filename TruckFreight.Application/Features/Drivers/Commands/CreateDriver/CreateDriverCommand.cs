using System;
using MediatR;
using FluentValidation;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Drivers.Commands.CreateDriver
{
    public class CreateDriverCommand : IRequest<Guid>
    {
        public Guid UserId { get; set; }
        public string LicenseNumber { get; set; }
        public DateTime LicenseExpiryDate { get; set; }
        public string LicenseType { get; set; }
        public string NationalId { get; set; }
        public string Address { get; set; }
        public string EmergencyContact { get; set; }
    }

    public class CreateDriverCommandValidator : AbstractValidator<CreateDriverCommand>
    {
        public CreateDriverCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.LicenseNumber).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LicenseExpiryDate).NotEmpty().GreaterThan(DateTime.UtcNow);
            RuleFor(x => x.LicenseType).NotEmpty().MaximumLength(20);
            RuleFor(x => x.NationalId).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Address).NotEmpty().MaximumLength(200);
            RuleFor(x => x.EmergencyContact).NotEmpty().MaximumLength(100);
        }
    }

    public class CreateDriverCommandHandler : IRequestHandler<CreateDriverCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public CreateDriverCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateDriverCommand request, CancellationToken cancellationToken)
        {
            var entity = new Driver(
                request.UserId,
                request.LicenseNumber,
                request.LicenseExpiryDate,
                request.LicenseType,
                request.NationalId,
                request.Address,
                request.EmergencyContact
            );
            _context.Drivers.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return entity.Id;
        }
    }
} 