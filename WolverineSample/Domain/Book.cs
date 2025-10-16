namespace Domain;

public class Book
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = default!;
    public string Author { get; set; } = default!;
    public string Isbn { get; set; } = default!;
    public DateOnly PublishedOn { get; set; }
    public decimal Price { get; set; }
}
