using AutoMapper;
using BookManager.Application.Events;
using BookManager.Application.Events.Dtos;
using BookManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BookManager.Infrastructure.Events;

public class BookEventDispatcher : BackgroundService
{
    private const int BatchSize = 50;

    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<BookEventDispatcher> _logger;

    public BookEventDispatcher(IServiceScopeFactory scopeFactory, TimeProvider timeProvider, ILogger<BookEventDispatcher> logger)
    {
        _scopeFactory = scopeFactory;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var dispatched = 0;

            try
            {
                dispatched = await DispatchBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to dispatch book events");
            }

            // A full batch means more are already waiting, so poll again without pausing.
            if (dispatched == BatchSize)
            {
                continue;
            }

            try
            {
                await Task.Delay(PollInterval, _timeProvider, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task<int> DispatchBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notifier = scope.ServiceProvider.GetRequiredService<IBookEventNotifier>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // SKIP LOCKED lets several instances drain the outbox without handing the same event to two of them.
        var events = await dbContext
            .BookEvents.FromSql(
                $"""
                SELECT * FROM "BookEvents"
                 WHERE "DispatchedAt" IS NULL
                 ORDER BY "Id"
                 LIMIT {BatchSize}
                   FOR UPDATE SKIP LOCKED
                """
            )
            .ToListAsync(cancellationToken);

        if (events.Count == 0)
        {
            return 0;
        }

        var dispatchedAt = _timeProvider.GetUtcNow();

        foreach (var bookEvent in events)
        {
            await notifier.PublishAsync(mapper.Map<BookEventDto>(bookEvent), cancellationToken);

            bookEvent.MarkDispatched(dispatchedAt);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return events.Count;
    }
}
