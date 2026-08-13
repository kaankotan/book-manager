using BookManager.Application.Repositories.Books;
using BookManager.Domain.Entities;
using BookManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookManager.Infrastructure.Repositories.Books;

public class BookRepository : IBookRepository
{
    private readonly AppDbContext _dbContext;

    public BookRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Book>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Books.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Books.AsNoTracking().FirstOrDefaultAsync(book => book.Id == id, cancellationToken);
    }

    public async Task AddAsync(Book book, CancellationToken cancellationToken = default)
    {
        await _dbContext.Books.AddAsync(book, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
