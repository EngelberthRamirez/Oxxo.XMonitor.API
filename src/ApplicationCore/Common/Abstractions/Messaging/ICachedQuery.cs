using MediatR;

namespace ApplicationCore.Common.Abstractions.Messaging
{
    public interface ICachedQuery<TResponse> : IRequest<TResponse>, ICachedQuery;

    public interface ICachedQuery
    {
        string Key { get; }
        TimeSpan? Expiration { get; }
    }
}
