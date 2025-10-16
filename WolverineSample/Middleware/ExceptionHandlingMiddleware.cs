using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _log = logger;

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (ValidationException vex)
        {
            // Map FluentValidation → 400 ProblemDetails
            var errors = vex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
            await Results.ValidationProblem(errors).ExecuteAsync(ctx);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unhandled exception");
            ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await Results.Problem(statusCode: 500, title: "Unhandled error").ExecuteAsync(ctx);
        }
    }
}
