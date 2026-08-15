using BookManager.Domain.Entities;

namespace BookManager.Application.Repositories.BookEvents;

public interface IBookEventRepository
{
    Task<IReadOnlyList<BookEvent>> GetPageAsync(Guid? bookId, long? before, int limit, CancellationToken cancellationToken = default);
}
