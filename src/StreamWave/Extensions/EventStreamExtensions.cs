namespace StreamWave.Extensions;

public static class EventStreamExtensions
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
