using BookManager.Application.Events;
using BookManager.Domain.Entities;

namespace BookManager.Application.Repositories.BookEvents;

public interface IBookEventRepository
{
    Task<IReadOnlyList<BookEvent>> GetPageAsync(
        Guid? bookId,
        long? before,
        long? since,
        int limit,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyList<BookEvent>> ListAsync(
        IReadOnlyList<Guid> bookIds,
        IReadOnlyList<BookChangeType> changeTypes,
        int skip,
        int take,
        BookEventSortField sortBy,
        bool descending,
        CancellationToken cancellationToken = default
    );

    Task<int> CountAsync(
        IReadOnlyList<Guid> bookIds,
        IReadOnlyList<BookChangeType> changeTypes,
        CancellationToken cancellationToken = default
    );

    Task<long?> GetLatestIdAsync(Guid bookId, CancellationToken cancellationToken = default);
}
