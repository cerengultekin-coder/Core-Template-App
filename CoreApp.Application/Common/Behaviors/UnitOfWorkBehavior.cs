using CoreApp.Application.Common.Interfaces;
using MediatR;

namespace CoreApp.Application.Common.Behaviors;

public sealed class UnitOfWorkBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IUnitOfWork _uow;

    public UnitOfWorkBehavior(IUnitOfWork uow) => _uow = uow;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var isCommand = typeof(TRequest).Name.EndsWith("Command", StringComparison.Ordinal);
        if (!isCommand)
            return await next();

        await _uow.BeginTransactionAsync(ct);
        try
        {
            var res = await next();
            await _uow.SaveChangesAsync(ct);
            await _uow.CommitAsync(ct);
            return res;
        }
        catch
        {
            await _uow.RollbackAsync(ct);
            throw;
        }
    }
}
