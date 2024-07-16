using MediatR;

namespace ApplicationCore.Common.Abstractions.Messaging
{
    public interface IQuery<TResponse> : IRequest<TResponse>
    {
    }
}
