using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using TruckFreight.Application.Common.Interfaces;
using TruckFreight.Application.Common.Exceptions;

namespace TruckFreight.Application.Common.Behaviors
{
    public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IAuthorize
    {
        private readonly ILogger<AuthorizationBehavior<TRequest, TResponse>> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IIdentityService _identityService;

        public AuthorizationBehavior(
            ILogger<AuthorizationBehavior<TRequest, TResponse>> logger,
            ICurrentUserService currentUserService,
            IIdentityService identityService)
        {
            _logger = logger;
            _currentUserService = currentUserService;
            _identityService = identityService;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var authorizeAttributes = request.GetType().GetCustomAttributes(typeof(AuthorizeAttribute), true);

            if (authorizeAttributes.Any())
            {
                var user = _currentUserService.UserId;

                if (user == null)
                {
                    throw new UnauthorizedAccessException();
                }

                var authorizeAttributesWithRoles = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(((AuthorizeAttribute)a).Roles));

                if (authorizeAttributesWithRoles.Any())
                {
                    var authorized = false;

                    foreach (var roles in authorizeAttributesWithRoles.Select(a => ((AuthorizeAttribute)a).Roles.Split(',')))
                    {
                        foreach (var role in roles)
                        {
                            var isInRole = await _identityService.IsInRoleAsync(user, role.Trim());
                            if (isInRole)
                            {
                                authorized = true;
                                break;
                            }
                        }
                    }

                    if (!authorized)
                    {
                        _logger.LogWarning("Authorization failed for user {UserId} on request {RequestType}", user, typeof(TRequest).Name);
                        throw new ForbiddenAccessException();
                    }
                }

                var authorizeAttributesWithPolicies = authorizeAttributes.Where(a => !string.IsNullOrWhiteSpace(((AuthorizeAttribute)a).Policy));

                if (authorizeAttributesWithPolicies.Any())
                {
                    foreach (var policy in authorizeAttributesWithPolicies.Select(a => ((AuthorizeAttribute)a).Policy))
                    {
                        var authorized = await _identityService.AuthorizeAsync(user, policy);

                        if (!authorized)
                        {
                            _logger.LogWarning("Authorization failed for user {UserId} on request {RequestType}", user, typeof(TRequest).Name);
                            throw new ForbiddenAccessException();
                        }
                    }
                }
            }

            return await next();
        }
    }
} 