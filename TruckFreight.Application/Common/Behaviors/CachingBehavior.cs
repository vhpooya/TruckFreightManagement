using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace TruckFreight.Application.Common.Behaviors
{
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICacheableQuery
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;
        private readonly MemoryCacheEntryOptions _cacheOptions;

        public CachingBehavior(IMemoryCache cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
        {
            _cache = cache;
            _logger = logger;
            _cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2))
                .SetPriority(CacheItemPriority.Normal);
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var cacheKey = request.CacheKey;

            if (_cache.TryGetValue(cacheKey, out TResponse cachedResponse))
            {
                _logger.LogInformation("Fetched from Cache -> '{Key}'", cacheKey);
                return cachedResponse;
            }

            var response = await next();

            _cache.Set(cacheKey, response, _cacheOptions);
            _logger.LogInformation("Added to Cache -> '{Key}'", cacheKey);

            return response;
        }
    }
} 