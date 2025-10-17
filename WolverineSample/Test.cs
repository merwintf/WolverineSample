// Program.cs
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

var builder = WebApplication.CreateBuilder(args);

// 1) Auto-discover validators (scans your assembly)
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// 2) Apply a global endpoint filter to a route group (or app-wide if you prefer)
var api = app.MapGroup("/");
api.AddEndpointFilter<AutoValidateFilter>();

// Example Minimal API endpoint (no manual validation needed)
api.MapPost("/users", (CreateUser req) => Results.Ok(new { Created = req.Email }));

app.Run();

// --- Models & Validators ---
public record CreateUser(string? Email, int Age);

public sealed class CreateUserValidator : AbstractValidator<CreateUser>
{
    public CreateUserValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Age).InclusiveBetween(18, 120);
    }
}

// 3) Global filter that validates *any* bound argument with a matching IValidator<T>
public sealed class AutoValidateFilter : IEndpointFilter
{
    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext ctx, EndpointFilterDelegate next)
    {
        var sp = ctx.HttpContext.RequestServices;

        foreach (var arg in ctx.Arguments)
        {
            if (arg is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
            var validator = sp.GetService(validatorType);
            if (validator is null) continue;

            // dynamic to call ValidateAsync(T)
            var validationResult = await ((dynamic)validator).ValidateAsync((dynamic)arg);
            if (!validationResult.IsValid)
            {
                // Shape into RFC7807-compatible validation problem response
                var errors = ((IEnumerable<FluentValidation.Results.ValidationFailure>)validationResult.Errors)
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return Results.ValidationProblem(errors); // 400 + application/problem+json
            }
        }

        return await next(ctx);
    }
}
