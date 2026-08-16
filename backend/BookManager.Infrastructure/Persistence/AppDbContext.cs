using BookManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookManager.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Book> Books => Set<Book>();

    public DbSet<Author> Authors => Set<Author>();

    public DbSet<BookEvent> BookEvents => Set<BookEvent>();

    public DbSet<BookView> BookViews => Set<BookView>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(book =>
        {
            book.Property(b => b.Title).HasMaxLength(Book.TitleMaxLength);

            book.Property(b => b.Description).HasMaxLength(Book.DescriptionMaxLength);

            book.Navigation(b => b.Authors).UsePropertyAccessMode(PropertyAccessMode.Field);

            book.HasMany(b => b.Authors).WithMany(a => a.Books).UsingEntity(join => join.ToTable("BookAuthors"));

            book.Ignore(b => b.PendingChanges);
        });

        modelBuilder.Entity<BookEvent>(bookEvent =>
        {
            bookEvent.Property(e => e.NewValue).HasMaxLength(Book.DescriptionMaxLength);

            bookEvent.HasIndex(e => new { e.BookId, e.Id }).IsDescending(false, true);

            bookEvent.HasIndex(e => e.DispatchedAt).HasFilter("\"DispatchedAt\" IS NULL");
        });

        modelBuilder.Entity<BookView>(bookView =>
        {
            bookView.HasKey(v => v.BookId);

            bookView.Property(v => v.BookId).ValueGeneratedNever();

            bookView.HasOne<Book>().WithOne().HasForeignKey<BookView>(v => v.BookId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Author>(author =>
        {
            author.Navigation(a => a.Books).UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }
}
