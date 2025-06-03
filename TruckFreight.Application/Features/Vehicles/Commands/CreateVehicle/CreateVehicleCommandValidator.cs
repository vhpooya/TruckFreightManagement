using FluentValidation;
using TruckFreight.Application.Common.Interfaces;

namespace TruckFreight.Application.Features.Vehicles.Commands.CreateVehicle
{
    public class CreateVehicleCommandValidator : AbstractValidator<CreateVehicleCommand>
    {
        private readonly IApplicationDbContext _context;

        public CreateVehicleCommandValidator(IApplicationDbContext context)
        {
            _context = context;

            RuleFor(v => v.PlateNumber)
                .NotEmpty().WithMessage("Plate number is required.")
                .MaximumLength(20).WithMessage("Plate number must not exceed 20 characters.")
                .MustAsync(BeUniquePlateNumber).WithMessage("The specified plate number already exists.");

            RuleFor(v => v.Type)
                .IsInEnum().WithMessage("Vehicle type is not valid.");

            RuleFor(v => v.Brand)
                .NotEmpty().WithMessage("Brand is required.")
                .MaximumLength(50).WithMessage("Brand must not exceed 50 characters.");

            RuleFor(v => v.Model)
                .NotEmpty().WithMessage("Model is required.")
                .MaximumLength(50).WithMessage("Model must not exceed 50 characters.");

            RuleFor(v => v.Year)
                .NotEmpty().WithMessage("Year is required.")
                .InclusiveBetween(1900, DateTime.UtcNow.Year).WithMessage("Year must be between 1900 and current year.");

            RuleFor(v => v.Color)
                .NotEmpty().WithMessage("Color is required.")
                .MaximumLength(30).WithMessage("Color must not exceed 30 characters.");

            RuleFor(v => v.VIN)
                .NotEmpty().WithMessage("VIN is required.")
                .Length(17).WithMessage("VIN must be exactly 17 characters.")
                .MustAsync(BeUniqueVIN).WithMessage("The specified VIN already exists.");

            RuleFor(v => v.EngineNumber)
                .NotEmpty().WithMessage("Engine number is required.")
                .MaximumLength(50).WithMessage("Engine number must not exceed 50 characters.")
                .MustAsync(BeUniqueEngineNumber).WithMessage("The specified engine number already exists.");

            RuleFor(v => v.Capacity)
                .NotEmpty().WithMessage("Capacity is required.")
                .GreaterThan(0).WithMessage("Capacity must be greater than 0.");

            RuleFor(v => v.CapacityUnit)
                .NotEmpty().WithMessage("Capacity unit is required.")
                .MaximumLength(20).WithMessage("Capacity unit must not exceed 20 characters.");

            RuleFor(v => v.Length)
                .NotEmpty().WithMessage("Length is required.")
                .GreaterThan(0).WithMessage("Length must be greater than 0.");

            RuleFor(v => v.Width)
                .NotEmpty().WithMessage("Width is required.")
                .GreaterThan(0).WithMessage("Width must be greater than 0.");

            RuleFor(v => v.Height)
                .NotEmpty().WithMessage("Height is required.")
                .GreaterThan(0).WithMessage("Height must be greater than 0.");

            RuleFor(v => v.DimensionUnit)
                .NotEmpty().WithMessage("Dimension unit is required.")
                .MaximumLength(20).WithMessage("Dimension unit must not exceed 20 characters.");
        }

        private async Task<bool> BeUniquePlateNumber(string plateNumber, CancellationToken cancellationToken)
        {
            return !await _context.Vehicles
                .AnyAsync(v => v.PlateNumber == plateNumber, cancellationToken);
        }

        private async Task<bool> BeUniqueVIN(string vin, CancellationToken cancellationToken)
        {
            return !await _context.Vehicles
                .AnyAsync(v => v.VIN == vin, cancellationToken);
        }

        private async Task<bool> BeUniqueEngineNumber(string engineNumber, CancellationToken cancellationToken)
        {
            return !await _context.Vehicles
                .AnyAsync(v => v.EngineNumber == engineNumber, cancellationToken);
        }
    }
} 