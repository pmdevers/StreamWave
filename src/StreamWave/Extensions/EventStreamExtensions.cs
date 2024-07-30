namespace StreamWave.Extensions;

/// <summary>
/// Provides extension methods for working with event streams.
/// </summary>
public static class EventStreamExtensions
{
    /// <summary>
    /// Aggregates a sequence of source elements asynchronously, applying a specified function to each element and accumulating the result.
    /// </summary>
    /// <typeparam name="TState">The type of the state being aggregated.</typeparam>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The sequence of source elements to aggregate.</param>
    /// <param name="aggregate">The initial state to start aggregation from.</param>
    /// <param name="func">The asynchronous function to apply to each element in the sequence.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the aggregated state.</returns>
    public static async Task<TState> AggregateAsync<TState, TSource>(
        this IEnumerable<TSource> source,
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
