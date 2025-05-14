using System.Transactions;
using MediatR;

namespace IndieSphere.Application.Behaviors;

public interface IAmTransactional
{
    IsolationLevel GetTransactionIsolationLevel() => IsolationLevel.ReadCommitted;
    TimeSpan GetTransactionTimeout() => TimeSpan.FromMinutes(2);
}


public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not IAmTransactional transactionalRequest) // basically just commands, but individual queries and jobs could add the marker interface if they needed it
        {
            return await next();
        }

        var options = new TransactionOptions
        {
            IsolationLevel = transactionalRequest.GetTransactionIsolationLevel(),
            Timeout = transactionalRequest.GetTransactionTimeout(),
        };

        using var scope = new TransactionScope(TransactionScopeOption.Required, options, TransactionScopeAsyncFlowOption.Enabled);

        var response = await next();

        scope.Complete();

        return response;
    }
}