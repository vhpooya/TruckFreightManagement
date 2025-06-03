using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace TruckFreight.Application.Common.Behaviors
{
    public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly Stopwatch _timer;
        private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
        private readonly long _longRunningThreshold;

        public PerformanceBehavior(
            ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
            long longRunningThreshold = 500)
        {
            _timer = new Stopwatch();
            _logger = logger;
            _longRunningThreshold = longRunningThreshold;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            _timer.Start();

            var response = await next();

            _timer.Stop();

            var elapsedMilliseconds = _timer.ElapsedMilliseconds;

            if (elapsedMilliseconds > _longRunningThreshold)
            {
                var requestName = typeof(TRequest).Name;
                _logger.LogWarning("Long Running Request: {Name} ({ElapsedMilliseconds} milliseconds) {@Request}",
                    requestName, elapsedMilliseconds, request);
            }

            return response;
        }
    }
} 