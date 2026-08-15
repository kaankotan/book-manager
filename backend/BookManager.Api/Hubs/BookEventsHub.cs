using Microsoft.AspNetCore.SignalR;

namespace BookManager.Api.Hubs;

/// <summary>
/// Clients only receive on this hub; there are no client-callable methods.
/// </summary>
public class BookEventsHub : Hub { }
