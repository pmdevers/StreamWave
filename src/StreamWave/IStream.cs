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


public static class StreamExtensions 
{
    public static async Task<TState> AggregateAsync<TState, TSource>
                                      (this IEnumerable<TSource> source,
                                       TState aggregate,
                                       Func<TState, TSource, Task<TState>> func)
    {
        using IEnumerator<TSource> e = source.GetEnumerator();
        if (!e.MoveNext())
        {
            return aggregate;
        }

        while (e.MoveNext()) aggregate = await func(aggregate, e.Current);
        return aggregate;
    }
}
