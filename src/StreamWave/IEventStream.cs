namespace StreamWave;

public interface IEventStream<TId> : IReadOnlyCollection<Event>
{
    TId Id { get; }
    bool HasUncommittedChanges { get; }
    int Version { get; }
    int ExpectedVersion { get; }
    DateTimeOffset? CreatedOn { get; }
    DateTimeOffset? LastModifiedOn { get; }
    void Append(Event e);
    Event[] GetUncommittedEvents();
    IEventStream<TId> Commit();
}
