using System.Collections;

namespace StreamWave;
/// <summary>
/// Provides methods to create event streams for aggregates.
/// </summary>
public static class EventStream
{
    /// <summary>
    /// Creates a new event stream for a given identifier and optionally includes initial events.
    /// </summary>
    /// <typeparam name="TId">The type of the identifier for the event stream.</typeparam>
    /// <param name="streamId">The identifier for the event stream.</param>
    /// <param name="events">An optional array of initial events.</param>
    /// <returns>An instance of <see cref="IEventStream{TId}"/>.</returns>
    public static IEventStream<TId> Create<TId>(TId streamId, IEnumerable<EventRecord>? events = null)
        => new EventStream<TId>(streamId, events);
}

/// <summary>
/// Represents an event stream for managing events in an event-sourced system.
/// </summary>
/// <typeparam name="TId">The type of the identifier for the event stream.</typeparam>
/// <param name="streamId">The identifier for the event stream.</param>
/// <param name="events">An optional array of initial events.</param>
public class EventStream<TId>(TId streamId, IEnumerable<EventRecord>? events) : IEventStream<TId>
{
    private readonly IEnumerable<EventRecord> _events = events ?? [];
    private readonly List<EventRecord> _uncommitted = [];
    private DateTimeOffset? _createdOn = null;
    private DateTimeOffset? _LastModifiedOn = null;

    /// <summary>
    /// Gets the identifier for the event stream.
    /// </summary>
    public TId Id { get; } = streamId;

    /// <summary>
    /// Indicates whether there are uncommitted changes in the event stream.
    /// </summary>
    public bool HasUncommittedChanges => _uncommitted.Count > 0;

    /// <summary>
    /// Gets the version of the event stream, based on the number of committed events.
    /// </summary>
    public int Version { get; private set; }

    /// <summary>
    /// Gets the expected version of the event stream, including uncommitted events.
    /// </summary>
    public int ExpectedVersion => Version + _uncommitted.Count;

    /// <summary>
    /// Gets the timestamp of when the first event was created, if available.
    /// </summary>
    public DateTimeOffset? CreatedOn => _createdOn ?? TimeProvider.System.GetUtcNow();

    /// <summary>
    /// Gets the timestamp of when the last event was modified, if available.
    /// </summary>
    public DateTimeOffset? LastModifiedOn => _LastModifiedOn ?? TimeProvider.System.GetUtcNow();

    /// <summary>
    /// Gets the total count of committed events in the event stream.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Appends a new event to the uncommitted events list.
    /// </summary>
    /// <param name="e">The event to be appended.</param>
    public void Append(object e)
        => _uncommitted.Add(EventRecord.Create(e));

    /// <summary>
    /// Gets the list of uncommitted events.
    /// </summary>
    /// <returns>An array of uncommitted events.</returns>
    public EventRecord[] GetUncommittedEvents() => [.. _uncommitted];

    /// <summary>
    /// Commits the uncommitted events, returning a new event stream with the changes.
    /// </summary>
    /// <returns>A new instance of <see cref="IEventStream{TId}"/> with the committed events.</returns>
    public IEventStream<TId> Commit() => EventStream.Create(Id, [.. _events, .. _uncommitted]);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async IAsyncEnumerator<EventRecord> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        foreach (var e in _events)
        {
            if(_createdOn == null)
            {
                _createdOn = e.OccurredOn;
            }
            _LastModifiedOn = e.OccurredOn;
            Count++;
            Version++;
            yield return e;
            await Task.Yield(); // Ensure it's asynchronous
        }
    }
}


/// <summary>
/// Represents an abstract base class for domain events in an event-sourced system.
/// </summary>
public record EventRecord(object Event, Type EventType, DateTimeOffset OccurredOn)
{
    /// <summary>
    /// Creates an event records
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public static EventRecord Create(object e)
        => new (e, e.GetType(), TimeProvider.System.GetUtcNow());
}
