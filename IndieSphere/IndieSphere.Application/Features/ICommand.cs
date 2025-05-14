using IndieSphere.Application.Behaviors;
using MediatR;

namespace IndieSphere.Application.Features;

/// <summary>
/// The interface that defines a contract for commands.
/// </summary>
/// <typeparam name="TResponse">The type of response returned from the command.</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>, IAmTransactional
{
}

public interface ICommand : IRequest, IAmTransactional
{
}
