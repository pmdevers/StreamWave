using System.Collections;

namespace StreamWave;

public static class EventStream
{
    public static IEventStream<TId> Create<TId>(TId streamId, Event[]? events = null)
        => new EventStream<TId>(streamId, events);
}

public class EventStream<TId>(TId streamId, Event[]? events = null) : IEventStream<TId>
{
    private readonly Event[] _events = events ?? [];
    private readonly List<Event> _uncommitted = [];

    public TId Id { get; } = streamId;

    public bool HasUncommittedChanges
       => _uncommitted.Count > 0;

    public int Version
        => _events.Length;

    public int ExpectedVersion
        => _events.Length + _uncommitted.Count;

    public DateTimeOffset? CreatedOn
        => _events.Length != 0
        ? _events.FirstOrDefault()?.OccouredOn
        : _uncommitted.FirstOrDefault()?.OccouredOn;

    public DateTimeOffset? LastModifiedOn
        => _uncommitted.Count != 0
        ? _uncommitted.LastOrDefault()?.OccouredOn
        : _events.LastOrDefault()?.OccouredOn;

    public int Count
        => _events.Length;

    public void Append(Event e)
        => _uncommitted.Add(e);

    public IEnumerator<Event> GetEnumerator()
        => _events.ToList().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public Event[] GetUncommittedEvents()
        => [.. _uncommitted];

    public IEventStream<TId> Commit()
        => EventStream.Create(Id, [.. _events, .. _uncommitted]);
}
