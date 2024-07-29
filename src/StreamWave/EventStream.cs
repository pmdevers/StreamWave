using System.Collections;

namespace StreamWave;

public class EventStream : IEventStream
{
    private readonly Event[] _events;
    private readonly List<Event> _uncommitted = [];

    private EventStream(Guid streamId, Event[]? events = null)
    {
        Id = streamId;
        _events = events ?? [];
    }

    public static IEventStream Create()
        => Create(StreamId.Guid());
    public static IEventStream Create(Guid streamId, Event[]? events = null)
        => new EventStream(streamId, events);

    public Guid Id { get; }

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

    public IEventStream Commit()
        => Create(Id, [.. _events, .. _uncommitted]);
}
