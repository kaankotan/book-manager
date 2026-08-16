using BookManager.Application.Repositories.BookEvents;
using BookManager.Domain.Entities;
using BookManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookManager.Infrastructure.Repositories.BookEvents;

public class BookEventRepository : IBookEventRepository
{
    private readonly AppDbContext _dbContext;

    public BookEventRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<BookEvent>> GetPageAsync(
        Guid? bookId,
        long? before,
        long? since,
        int limit,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbContext.BookEvents.AsNoTracking();

        if (bookId is not null)
        {
            query = query.Where(bookEvent => bookEvent.BookId == bookId);
        }

        if (before is not null)
        {
            query = query.Where(bookEvent => bookEvent.Id < before);
        }

        if (since is not null)
        {
            query = query.Where(bookEvent => bookEvent.Id > since);
        }

        // One extra row tells the caller whether a further page exists, without a second round-trip.
        return await query.OrderByDescending(bookEvent => bookEvent.Id).Take(limit + 1).ToListAsync(cancellationToken);
    }

    public async Task<long?> GetLatestIdAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        return await _dbContext
            .BookEvents.AsNoTracking()
            .Where(bookEvent => bookEvent.BookId == bookId)
            .OrderByDescending(bookEvent => bookEvent.Id)
            .Select(bookEvent => (long?)bookEvent.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
