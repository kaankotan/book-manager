using BookManager.Application.Events.Dtos;

namespace BookManager.Application.Events;

public interface IBookEventNotifier
{
    Task PublishAsync(BookEventDto bookEvent, CancellationToken cancellationToken = default);
}
