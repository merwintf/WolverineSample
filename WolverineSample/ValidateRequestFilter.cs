using FluentValidation;

public sealed class ValidateRequestFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        var validator = ctx.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is null) return await next(ctx); // no validator, proceed

        var dto = ctx.GetArgument<T>(0); // first arg should be your request DTO
        var result = await validator.ValidateAsync(dto);
        if (!result.IsValid) return Results.ValidationProblem(result.ToDictionary());

        return await next(ctx);
    }
}
