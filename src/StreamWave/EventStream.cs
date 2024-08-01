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
    public static IEventStream<TId> Create<TId>(TId streamId, EventRecord[]? events = null)
        => new EventStream<TId>(streamId, events);
}

/// <summary>
/// Represents an event stream for managing events in an event-sourced system.
/// </summary>
/// <typeparam name="TId">The type of the identifier for the event stream.</typeparam>
/// <param name="streamId">The identifier for the event stream.</param>
/// <param name="events">An optional array of initial events.</param>
public class EventStream<TId>(TId streamId, EventRecord[]? events = null) : IEventStream<TId>
{
    private readonly EventRecord[] _events = events ?? [];
    private readonly List<EventRecord> _uncommitted = [];

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
    public int Version => _events.Length;

    /// <summary>
    /// Gets the expected version of the event stream, including uncommitted events.
    /// </summary>
    public int ExpectedVersion => _events.Length + _uncommitted.Count;

    /// <summary>
    /// Gets the timestamp of when the first event was created, if available.
    /// </summary>
    public DateTimeOffset? CreatedOn => _events.Length != 0
        ? _events.FirstOrDefault()?.OccurredOn
        : _uncommitted.FirstOrDefault()?.OccurredOn;

    /// <summary>
    /// Gets the timestamp of when the last event was modified, if available.
    /// </summary>
    public DateTimeOffset? LastModifiedOn => _uncommitted.Count != 0
        ? _uncommitted.LastOrDefault()?.OccurredOn
        : _events.LastOrDefault()?.OccurredOn;

    /// <summary>
    /// Gets the total count of committed events in the event stream.
    /// </summary>
    public int Count => _events.Length;

    /// <summary>
    /// Appends a new event to the uncommitted events list.
    /// </summary>
    /// <param name="e">The event to be appended.</param>
    public void Append(object e)
        => _uncommitted.Add(EventRecord.Create(e));

    /// <summary>
    /// Returns an enumerator that iterates through the committed events.
    /// </summary>
    /// <returns>An enumerator for the committed events.</returns>
    public IEnumerator<EventRecord> GetEnumerator() => _events.ToList().GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through the committed events (non-generic).
    /// </summary>
    /// <returns>An enumerator for the committed events.</returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

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
