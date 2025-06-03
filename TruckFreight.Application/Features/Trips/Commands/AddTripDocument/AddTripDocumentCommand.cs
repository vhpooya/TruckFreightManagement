using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FluentValidation;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;
using TruckFreight.Domain.Entities;

namespace TruckFreight.Application.Features.Trips.Commands.AddTripDocument
{
    public class AddTripDocumentCommand : IRequest<Result<Guid>>
    {
        public Guid TripId { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileUrl { get; set; }
        public string Notes { get; set; }
    }

    public class AddTripDocumentCommandValidator : AbstractValidator<AddTripDocumentCommand>
    {
        public AddTripDocumentCommandValidator()
        {
            RuleFor(x => x.TripId).NotEmpty();
            RuleFor(x => x.FileName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.FileType).NotEmpty().MaximumLength(50);
            RuleFor(x => x.FileUrl).NotEmpty().MaximumLength(500);
        }
    }

    public class AddTripDocumentCommandHandler : IRequestHandler<AddTripDocumentCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public AddTripDocumentCommandHandler(
            IApplicationDbContext context,
            ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<Result<Guid>> Handle(AddTripDocumentCommand request, CancellationToken cancellationToken)
        {
            var trip = await _context.Trips.FindAsync(request.TripId);

            if (trip == null)
            {
                throw new NotFoundException(nameof(Trip), request.TripId);
            }

            if (!_currentUserService.IsAdmin && trip.DriverId != _currentUserService.UserId)
            {
                throw new ForbiddenAccessException();
            }

            var document = new TripDocument
            {
                TripId = request.TripId,
                FileName = request.FileName,
                FileType = request.FileType,
                FileUrl = request.FileUrl,
                Notes = request.Notes,
                UploadedAt = DateTime.UtcNow,
                UploadedBy = _currentUserService.UserId
            };

            _context.TripDocuments.Add(document);
            await _context.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(document.Id, "Document added successfully");
        }
    }
} 