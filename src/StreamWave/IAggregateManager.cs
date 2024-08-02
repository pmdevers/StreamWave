namespace StreamWave;

/// <summary>
/// Interface for managing aggregates, including creation, loading, and saving operations.
/// </summary>
/// <typeparam name="TState">The type of the aggregate state.</typeparam>
/// <typeparam name="TId">The type of the identifier used for the aggregate.</typeparam>
public interface IAggregateManager<TState, TId>
{
    /// <summary>
    /// Creates a new aggregate with the initial state.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with the newly created aggregate as the result.</returns>
    IAggregate<TState, TId> Create();

    /// <summary>
    /// Loads an existing aggregate based on its identifier.
    /// </summary>
    /// <param name="id">The identifier of the aggregate to be loaded.</param>
    /// <returns>A task representing the asynchronous operation, with the loaded aggregate as the result.</returns>
    Task<IAggregate<TState, TId>> LoadAsync(TId id);

    /// <summary>
    /// Saves the current state of the specified aggregate.
    /// </summary>
    /// <param name="aggregate">The aggregate to be saved.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveAsync(IAggregate<TState, TId> aggregate);
}
