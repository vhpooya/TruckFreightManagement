using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace TruckFreight.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var uniqueId = Guid.NewGuid().ToString();

            _logger.LogInformation("Begin Request {UniqueId}: {Name} {@Request}",
                uniqueId, requestName, request);

            var timer = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var response = await next();
                timer.Stop();

                _logger.LogInformation("End Request {UniqueId}: {Name} ({ElapsedMilliseconds}ms) {@Response}",
                    uniqueId, requestName, timer.ElapsedMilliseconds, response);

                return response;
            }
            catch (Exception ex)
            {
                timer.Stop();
                _logger.LogError(ex, "Request {UniqueId}: {Name} failed ({ElapsedMilliseconds}ms) {@Request}",
                    uniqueId, requestName, timer.ElapsedMilliseconds, request);
                throw;
            }
        }
    }
} 