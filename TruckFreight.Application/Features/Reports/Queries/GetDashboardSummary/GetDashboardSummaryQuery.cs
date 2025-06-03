using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using TruckFreight.Application.Common.Exceptions;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Models;

namespace TruckFreight.Application.Features.Reports.Queries.GetDashboardSummary
{
    public class GetDashboardSummaryQuery : IRequest<Result<DashboardSummaryDto>>
    {
        public string CompanyId { get; set; }
    }

    public class GetDashboardSummaryQueryValidator : AbstractValidator<GetDashboardSummaryQuery>
    {
        public GetDashboardSummaryQueryValidator()
        {
            RuleFor(x => x.CompanyId)
                .NotEmpty().WithMessage("Company ID is required");
        }
    }

    public class GetDashboardSummaryQueryHandler : IRequestHandler<GetDashboardSummaryQuery, Result<DashboardSummaryDto>>
    {
        private readonly IReportingService _reportingService;
        private readonly ICurrentUserService _currentUserService;

        public GetDashboardSummaryQueryHandler(
            IReportingService reportingService,
            ICurrentUserService currentUserService)
        {
            _reportingService = reportingService;
            _currentUserService = currentUserService;
        }

        public async Task<Result<DashboardSummaryDto>> Handle(
            GetDashboardSummaryQuery request,
            CancellationToken cancellationToken)
        {
            // Validate user has access to the company's data
            if (_currentUserService.UserId != request.CompanyId && 
                !await _currentUserService.IsInRoleAsync("Admin"))
            {
                throw new ForbiddenAccessException();
            }

            return await _reportingService.GetDashboardSummaryAsync(request.CompanyId);
        }
    }
} 