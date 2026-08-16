using BookManager.Application.Repositories.BookViews;
using BookManager.Domain.Entities;
using BookManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookManager.Infrastructure.Repositories.BookViews;

public class BookViewRepository : IBookViewRepository
{
    private readonly AppDbContext _dbContext;

    public BookViewRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<BookView?> GetAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.BookViews.AsNoTracking().FirstOrDefaultAsync(view => view.BookId == bookId, cancellationToken);
    }

    public async Task<BookView?> GetTrackedAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.BookViews.FirstOrDefaultAsync(view => view.BookId == bookId, cancellationToken);
    }

    public async Task AddAsync(BookView bookView, CancellationToken cancellationToken = default)
    {
        await _dbContext.BookViews.AddAsync(bookView, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
