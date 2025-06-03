using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Swashbuckle.AspNetCore.Annotations;
using TruckFreight.Application.Common.Models;
using TruckFreight.Application.Features.Reports.Queries.GetCargoTypeReport;
using TruckFreight.Application.Features.Reports.Queries.GetDashboardSummary;
using TruckFreight.Application.Features.Reports.Queries.GetDeliveryReport;
using TruckFreight.Application.Features.Reports.Queries.GetDriverPerformanceReport;
using TruckFreight.Application.Features.Reports.Queries.GetFinancialReport;
using TruckFreight.Application.Features.Reports.Queries.GetRouteAnalysisReport;

namespace TruckFreight.API.Controllers
{
    [Authorize]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Produces("application/json")]
    public class ReportsController : ApiControllerBase
    {
        private readonly IMemoryCache _cache;
        private const int CacheDurationMinutes = 5;

        public ReportsController(IMemoryCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Gets delivery statistics and trends for a company within a date range
        /// </summary>
        /// <param name="companyId">The ID of the company</param>
        /// <param name="startDate">Start date of the report period</param>
        /// <param name="endDate">End date of the report period</param>
        /// <returns>Delivery report with statistics and trends</returns>
        [HttpGet("delivery")]
        [ResponseCache(Duration = 300)] // 5 minutes
        [ProducesResponseType(typeof(Result<DeliveryReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Get delivery report",
            Description = "Retrieves delivery statistics and trends for a company within a specified date range",
            OperationId = "GetDeliveryReport",
            Tags = new[] { "Reports" }
        )]
        public async Task<ActionResult<Result<DeliveryReportDto>>> GetDeliveryReport(
            [FromQuery] string companyId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var cacheKey = $"delivery_report_{companyId}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
            
            if (_cache.TryGetValue(cacheKey, out Result<DeliveryReportDto> cachedResult))
            {
                return cachedResult;
            }

            var query = new GetDeliveryReportQuery
            {
                CompanyId = companyId,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await Mediator.Send(query);
            
            if (result.Succeeded)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return result;
        }

        /// <summary>
        /// Gets financial data and breakdowns for a company within a date range
        /// </summary>
        [HttpGet("financial")]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(Result<FinancialReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Get financial report",
            Description = "Retrieves financial data and breakdowns for a company within a specified date range",
            OperationId = "GetFinancialReport",
            Tags = new[] { "Reports" }
        )]
        public async Task<ActionResult<Result<FinancialReportDto>>> GetFinancialReport(
            [FromQuery] string companyId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var cacheKey = $"financial_report_{companyId}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
            
            if (_cache.TryGetValue(cacheKey, out Result<FinancialReportDto> cachedResult))
            {
                return cachedResult;
            }

            var query = new GetFinancialReportQuery
            {
                CompanyId = companyId,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await Mediator.Send(query);
            
            if (result.Succeeded)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return result;
        }

        /// <summary>
        /// Gets driver performance metrics for a company within a date range
        /// </summary>
        [HttpGet("driver-performance")]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(Result<DriverPerformanceReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Get driver performance report",
            Description = "Retrieves driver performance metrics for a company within a specified date range",
            OperationId = "GetDriverPerformanceReport",
            Tags = new[] { "Reports" }
        )]
        public async Task<ActionResult<Result<DriverPerformanceReportDto>>> GetDriverPerformanceReport(
            [FromQuery] string companyId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var cacheKey = $"driver_performance_{companyId}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
            
            if (_cache.TryGetValue(cacheKey, out Result<DriverPerformanceReportDto> cachedResult))
            {
                return cachedResult;
            }

            var query = new GetDriverPerformanceReportQuery
            {
                CompanyId = companyId,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await Mediator.Send(query);
            
            if (result.Succeeded)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return result;
        }

        /// <summary>
        /// Gets cargo type analysis and trends for a company within a date range
        /// </summary>
        [HttpGet("cargo-type")]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(Result<CargoTypeReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Get cargo type report",
            Description = "Retrieves cargo type analysis and trends for a company within a specified date range",
            OperationId = "GetCargoTypeReport",
            Tags = new[] { "Reports" }
        )]
        public async Task<ActionResult<Result<CargoTypeReportDto>>> GetCargoTypeReport(
            [FromQuery] string companyId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var cacheKey = $"cargo_type_{companyId}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
            
            if (_cache.TryGetValue(cacheKey, out Result<CargoTypeReportDto> cachedResult))
            {
                return cachedResult;
            }

            var query = new GetCargoTypeReportQuery
            {
                CompanyId = companyId,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await Mediator.Send(query);
            
            if (result.Succeeded)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return result;
        }

        /// <summary>
        /// Gets route performance and analysis for a company within a date range
        /// </summary>
        [HttpGet("route-analysis")]
        [ResponseCache(Duration = 300)]
        [ProducesResponseType(typeof(Result<RouteAnalysisReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Get route analysis report",
            Description = "Retrieves route performance and analysis for a company within a specified date range",
            OperationId = "GetRouteAnalysisReport",
            Tags = new[] { "Reports" }
        )]
        public async Task<ActionResult<Result<RouteAnalysisReportDto>>> GetRouteAnalysisReport(
            [FromQuery] string companyId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var cacheKey = $"route_analysis_{companyId}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
            
            if (_cache.TryGetValue(cacheKey, out Result<RouteAnalysisReportDto> cachedResult))
            {
                return cachedResult;
            }

            var query = new GetRouteAnalysisReportQuery
            {
                CompanyId = companyId,
                StartDate = startDate,
                EndDate = endDate
            };

            var result = await Mediator.Send(query);
            
            if (result.Succeeded)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return result;
        }

        /// <summary>
        /// Gets dashboard summary with key metrics for a company
        /// </summary>
        [HttpGet("dashboard")]
        [ResponseCache(Duration = 60)] // 1 minute for dashboard
        [ProducesResponseType(typeof(Result<DashboardSummaryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Get dashboard summary",
            Description = "Retrieves dashboard summary with key metrics for a company",
            OperationId = "GetDashboardSummary",
            Tags = new[] { "Reports" }
        )]
        public async Task<ActionResult<Result<DashboardSummaryDto>>> GetDashboardSummary(
            [FromQuery] string companyId)
        {
            var cacheKey = $"dashboard_{companyId}";
            
            if (_cache.TryGetValue(cacheKey, out Result<DashboardSummaryDto> cachedResult))
            {
                return cachedResult;
            }

            var query = new GetDashboardSummaryQuery
            {
                CompanyId = companyId
            };

            var result = await Mediator.Send(query);
            
            if (result.Succeeded)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(1)); // Shorter cache for dashboard
            }

            return result;
        }
    }
} 