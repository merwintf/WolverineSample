using Middleware;

namespace Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        => app.UseMiddleware<ExceptionHandlingMiddleware>();

    public static IApplicationBuilder UseUnitOfWorkTransactions(this IApplicationBuilder app)
        => app.UseMiddleware<UnitOfWorkTransactionMiddleware>();
}
