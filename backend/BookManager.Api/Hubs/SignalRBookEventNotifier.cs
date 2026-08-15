using BookManager.Application.Events;
using BookManager.Application.Events.Dtos;
using Microsoft.AspNetCore.SignalR;

namespace BookManager.Api.Hubs;

public class SignalRBookEventNotifier : IBookEventNotifier
{
    public const string BookEventCreated = "BookEventCreated";

    private readonly IHubContext<BookEventsHub> _hubContext;

    public SignalRBookEventNotifier(IHubContext<BookEventsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task PublishAsync(BookEventDto bookEvent, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients.All.SendAsync(BookEventCreated, bookEvent, cancellationToken);
    }
}
