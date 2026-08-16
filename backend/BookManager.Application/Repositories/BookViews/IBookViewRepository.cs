using BookManager.Domain.Entities;

namespace BookManager.Application.Repositories.BookViews;

public interface IBookViewRepository
{
    Task<BookView?> GetAsync(Guid bookId, CancellationToken cancellationToken = default);

    Task<BookView?> GetTrackedAsync(Guid bookId, CancellationToken cancellationToken = default);

    Task AddAsync(BookView bookView, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
