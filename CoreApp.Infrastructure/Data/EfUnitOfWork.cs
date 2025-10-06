using CoreApp.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace CoreApp.Infrastructure.Data;

public sealed class EfUnitOfWork(CoreAppDbContext ctx) : IUnitOfWork, IAsyncDisposable
{
    private readonly CoreAppDbContext _ctx = ctx;
    private IDbContextTransaction? _tx;

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        if (_tx != null) return;
        _tx = await _ctx.Database.BeginTransactionAsync(ct);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _ctx.SaveChangesAsync(ct);

    public async Task CommitAsync(CancellationToken ct = default)
    {
        await _ctx.SaveChangesAsync(ct);
        if (_tx != null)
        {
            await _tx.CommitAsync(ct);
            await _tx.DisposeAsync();
            _tx = null;
        }
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_tx != null)
        {
            await _tx.RollbackAsync(ct);
            await _tx.DisposeAsync();
            _tx = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_tx != null)
            await _tx.DisposeAsync();
    }
}
