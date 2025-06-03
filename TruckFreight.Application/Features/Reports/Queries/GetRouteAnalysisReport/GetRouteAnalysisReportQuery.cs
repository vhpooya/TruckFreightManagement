using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;

namespace TruckFreight.Application.Features.Reports.Queries.GetRouteAnalysisReport
{
    public class GetRouteAnalysisReportQuery : IRequest<Result<RouteAnalysisReportDto>>
    {
        public string CompanyId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class GetRouteAnalysisReportQueryValidator : AbstractValidator<GetRouteAnalysisReportQuery>
    {
        public GetRouteAnalysisReportQueryValidator()
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

    public class GetRouteAnalysisReportQueryHandler : IRequestHandler<GetRouteAnalysisReportQuery, Result<RouteAnalysisReportDto>>
    {
        private readonly IReportingService _reportingService;
        private readonly ICurrentUserService _currentUserService;

        public GetRouteAnalysisReportQueryHandler(
            IReportingService reportingService,
            ICurrentUserService currentUserService)
        {
            _reportingService = reportingService;
            _currentUserService = currentUserService;
        }

        public async Task<Result<RouteAnalysisReportDto>> Handle(
            GetRouteAnalysisReportQuery request,
            CancellationToken cancellationToken)
        {
            // Validate user has access to the company's data
            if (_currentUserService.UserId != request.CompanyId && 
                !await _currentUserService.IsInRoleAsync("Admin"))
            {
                throw new ForbiddenAccessException();
            }

            return await _reportingService.GetRouteAnalysisReportAsync(
                request.CompanyId,
                request.StartDate,
                request.EndDate);
        }
    }
} 