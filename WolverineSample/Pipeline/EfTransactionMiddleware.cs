using Infra;
using Microsoft.EntityFrameworkCore.Storage;

namespace Pipeline;

public sealed class EfTransactionMiddleware
{
    private readonly AppDbContext _db;
    private IDbContextTransaction? _tx;
    private bool _committed;

    public EfTransactionMiddleware(AppDbContext db) => _db = db;

    // Runs before the handler
    public async Task BeforeAsync(CancellationToken ct)
    {
        _tx = await _db.Database.BeginTransactionAsync(ct);
        _committed = false;
    }

    // Runs after the handler only if no exception
    public async Task AfterAsync(CancellationToken ct)
    {
        await _db.SaveChangesAsync(ct);
        if (_tx is not null)
        {
            await _tx.CommitAsync(ct);
            _committed = true;
        }
    }

    // Always runs; if AfterAsync didn’t run, roll back
    public async Task FinallyAsync(CancellationToken ct)
    {
        if (_tx is null) return;

        if (!_committed)
            await _tx.RollbackAsync(ct);

        await _tx.DisposeAsync();
    }
}
