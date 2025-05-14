using MediatR;

namespace IndieSphere.Application.Features;

/// <summary>
/// Contract for creating a query which returns a response.
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
