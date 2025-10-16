using Infra;
using Microsoft.EntityFrameworkCore;

namespace Middleware;

public sealed class UnitOfWorkTransactionMiddleware(RequestDelegate next)
{
    private static readonly HashSet<string> WriteMethods = new(["POST", "PUT", "PATCH", "DELETE"], StringComparer.OrdinalIgnoreCase);
    private readonly RequestDelegate _next = next;

    public async Task InvokeAsync(HttpContext http, AppDbContext db)
    {
        // Read-only → pass through
        if (!WriteMethods.Contains(http.Request.Method))
        {
            await _next(http);
            return;
        }

        await using var tx = await db.Database.BeginTransactionAsync(http.RequestAborted);
        try
        {
            await _next(http);

            // Commit only on success (status < 400)
            if (http.Response.StatusCode < 400)
            {
                await tx.CommitAsync(http.RequestAborted);
            }
            else
            {
                await tx.RollbackAsync(http.RequestAborted);
            }
        }
        catch
        {
            await tx.RollbackAsync(http.RequestAborted);
            throw;
        }
    }
}
