using BookManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookManager.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Book> Books => Set<Book>();
}
