using ApplicationCore.Common.Abstractions.Caching;
using ApplicationCore.Common.Abstractions.Messaging;
using MediatR;

namespace ApplicationCore.Common.Behaviours;

public class QueryCachingPipeLineBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
     where TRequest : ICachedQuery
{
    private readonly ICacheService _cacheService;

    public QueryCachingPipeLineBehavior(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        return await _cacheService.GetOrCreateAsync(
            request.Key,
            _ => next(),
            request.Expiration,
            cancellationToken);
    }
}
