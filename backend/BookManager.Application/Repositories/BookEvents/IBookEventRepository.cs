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

    Task<long?> GetLatestIdAsync(Guid bookId, CancellationToken cancellationToken = default);
}
