using BookManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookManager.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Book> Books => Set<Book>();

    public DbSet<Author> Authors => Set<Author>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(book =>
        {
            book.Navigation(b => b.Authors).UsePropertyAccessMode(PropertyAccessMode.Field);

            book.HasMany(b => b.Authors).WithMany(a => a.Books).UsingEntity(join => join.ToTable("BookAuthors"));
        });

        modelBuilder.Entity<Author>(author =>
        {
            author.Navigation(a => a.Books).UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }
}
