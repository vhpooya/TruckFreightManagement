using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;

namespace TruckFreight.Application.Features.Reports.Queries.GetCargoTypeReport
{
    public class GetCargoTypeReportQuery : IRequest<Result<CargoTypeReportDto>>
    {
        public string CompanyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class GetCargoTypeReportQueryValidator : AbstractValidator<GetCargoTypeReportQuery>
    {
        public GetCargoTypeReportQueryValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("Company ID is required");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required")
                .LessThanOrEqualTo(x => x.EndDate)
                .WithMessage("Start date must be less than or equal to end date");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("End date must be greater than or equal to start date");
        }
    }

    public class GetCargoTypeReportQueryHandler : IRequestHandler<GetCargoTypeReportQuery, Result<CargoTypeReportDto>>
    {
        private readonly IReportingService _reportingService;
        private readonly ICurrentUserService _currentUserService;

        public GetCargoTypeReportQueryHandler(
            IReportingService reportingService,
            ICurrentUserService currentUserService)
        {
            _reportingService = reportingService;
            _currentUserService = currentUserService;
        }

        public async Task<Result<CargoTypeReportDto>> Handle(
            GetCargoTypeReportQuery request,
            CancellationToken cancellationToken)
        {
            // Validate user has access to the company's data
            if (_currentUserService.UserId != request.CompanyId && 
                !await _currentUserService.IsInRoleAsync("Admin"))
            {
                throw new ForbiddenAccessException();
            }

            return await _reportingService.GetCargoTypeReportAsync(
                request.CompanyId,
                request.StartDate,
                request.EndDate);
        }
    }
} 