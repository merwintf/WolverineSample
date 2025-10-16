using Domain;
using FluentValidation;
using Infra;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace Features.Books.Add;

public record AddBook(string Title, string Author, string Isbn, DateOnly PublishedOn, decimal Price);

public static class Endpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/books", async (AddBook cmd, IMessageBus bus, CancellationToken ct) =>
        {
            var id = await bus.InvokeAsync<Guid>(cmd, ct);
            return Results.Created($"/api/books/{id}", new { id });
        })
        .WithName("AddBook")
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}

public sealed class AddBookValidator : AbstractValidator<AddBook>
{
    public AddBookValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Author).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Isbn).NotEmpty().MaximumLength(20)
            .Matches(@"^[0-9\-Xx]+$").WithMessage("ISBN must contain digits, dashes, or X.");
        RuleFor(x => x.PublishedOn).LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.Date));
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0m);
    }
}

public static class Handler
{
    // Transaction + SaveChanges handled by pipeline middleware.
    public static async Task<Guid> Handle(AddBook cmd, AppDbContext db, CancellationToken ct)
    {
        var exists = await db.Books.AnyAsync(b => b.Isbn == cmd.Isbn, ct);
        if (exists)
            throw new FluentValidation.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(cmd.Isbn), "ISBN already exists.")
            });

        var entity = new Book
        {
            Title = cmd.Title,
            Author = cmd.Author,
            Isbn = cmd.Isbn,
            PublishedOn = cmd.PublishedOn,
            Price = cmd.Price
        };

        db.Books.Add(entity);
        throw new Exception("Test");
        return entity.Id;
    }
}
