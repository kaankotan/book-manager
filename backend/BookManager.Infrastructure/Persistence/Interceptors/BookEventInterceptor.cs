using BookManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BookManager.Infrastructure.Persistence.Interceptors;

public class BookEventInterceptor : SaveChangesInterceptor
{
    private readonly TimeProvider _timeProvider;

    public BookEventInterceptor(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        AddBookEvents(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        AddBookEvents(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddBookEvents(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var occurredAt = _timeProvider.GetUtcNow();

        var books = context
            .ChangeTracker.Entries<Book>()
            .Select(entry => entry.Entity)
            .Where(book => book.PendingChanges.Count > 0)
            .ToList();

        foreach (var book in books)
        {
            foreach (var change in book.PendingChanges)
            {
                context.Add(new BookEvent(book.Id, change.ChangeType, change.NewValue, occurredAt));
            }

            book.ClearPendingChanges();
        }
    }
}
