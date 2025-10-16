using Microsoft.EntityFrameworkCore;

namespace Infra;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Domain.Book> Books => Set<Domain.Book>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Domain.Book>(e =>
        {
            e.ToTable("Books"); // match exact casing on MySQL
            e.HasKey(x => x.Id);

            e.Property(x => x.Id)
                .HasColumnType("char(36)");

            e.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            e.Property(x => x.Author)
                .IsRequired()
                .HasMaxLength(120);

            e.Property(x => x.Isbn)
                .IsRequired()
                .HasMaxLength(20);
            e.HasIndex(x => x.Isbn).IsUnique();

            e.Property(x => x.PublishedOn)
                .HasColumnType("date");

            e.Property(x => x.Price)
                .HasPrecision(10, 2);
        });
    }
}
