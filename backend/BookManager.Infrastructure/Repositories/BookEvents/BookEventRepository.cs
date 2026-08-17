using System.Linq.Expressions;
using BookManager.Application.Events;
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
        var query = ForBook(bookId);

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

    public async Task<IReadOnlyList<BookEvent>> ListAsync(
        IReadOnlyList<Guid> bookIds,
        IReadOnlyList<BookChangeType> changeTypes,
        int skip,
        int take,
        BookEventSortField sortBy,
        bool descending,
        CancellationToken cancellationToken = default
    )
    {
        var filtered = Filtered(bookIds, changeTypes);

        return await Ordered(filtered, sortBy, descending).Skip(skip).Take(take).ToListAsync(cancellationToken);
    }

    public async Task<int> CountAsync(
        IReadOnlyList<Guid> bookIds,
        IReadOnlyList<BookChangeType> changeTypes,
        CancellationToken cancellationToken = default
    )
    {
        return await Filtered(bookIds, changeTypes).CountAsync(cancellationToken);
    }

    public async Task<long?> GetLatestIdAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        return await ForBook(bookId)
            .OrderByDescending(bookEvent => bookEvent.Id)
            .Select(bookEvent => (long?)bookEvent.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    private IQueryable<BookEvent> ForBook(Guid? bookId)
    {
        var query = _dbContext.BookEvents.AsNoTracking();

        return bookId is null ? query : query.Where(bookEvent => bookEvent.BookId == bookId);
    }

    private IQueryable<BookEvent> Filtered(IReadOnlyList<Guid> bookIds, IReadOnlyList<BookChangeType> changeTypes)
    {
        var query = _dbContext.BookEvents.AsNoTracking();

        if (bookIds.Count > 0)
        {
            query = query.Where(bookEvent => bookIds.Contains(bookEvent.BookId));
        }

        if (changeTypes.Count > 0)
        {
            query = query.Where(bookEvent => changeTypes.Contains(bookEvent.ChangeType));
        }

        return query;
    }

    private IQueryable<BookEvent> Ordered(IQueryable<BookEvent> query, BookEventSortField sortBy, bool descending)
    {
        Expression<Func<BookEvent, string?>> bookTitle = bookEvent =>
            _dbContext.Books.Where(book => book.Id == bookEvent.BookId).Select(book => book.Title).FirstOrDefault();

        // Id breaks ties so that a row never shifts between pages while paging through equal values.
        return (sortBy, descending) switch
        {
            (BookEventSortField.BookTitle, true) => query.OrderByDescending(bookTitle).ThenByDescending(bookEvent => bookEvent.Id),
            (BookEventSortField.BookTitle, false) => query.OrderBy(bookTitle).ThenBy(bookEvent => bookEvent.Id),
            (_, true) => query.OrderByDescending(bookEvent => bookEvent.OccurredAt).ThenByDescending(bookEvent => bookEvent.Id),
            (_, false) => query.OrderBy(bookEvent => bookEvent.OccurredAt).ThenBy(bookEvent => bookEvent.Id),
        };
    }
}
