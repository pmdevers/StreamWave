namespace StreamWave;

/// <summary>
/// Represents an aggregate in a domain-driven design context, encapsulating state and behavior.
/// </summary>
/// <typeparam name="TState">The type of the state associated with the aggregate.</typeparam>
/// <typeparam name="TId">The type of the identifier for the aggregate.</typeparam>
public interface IAggregate<out TState, TId>
{
    /// <summary>
    /// Gets the current state of the aggregate.
    /// </summary>
    TState State { get; }

    /// <summary>
    /// Gets the event stream associated with the aggregate, containing events that represent state changes.
    /// </summary>
    IEventStream<TId> Stream { get; }

    /// <summary>
    /// Gets the validation messages for the current state of the aggregate.
    /// </summary>
    ValidationMessage[] Messages { get; }

    /// <summary>
    /// Gets a value indicating whether the current state of the aggregate is valid.
    /// </summary>
    bool IsValid { get; }

    /// <summary>
    /// Applies a new event to the aggregate, updating its state asynchronously.
    /// </summary>
    /// <param name="e">The event to apply.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ApplyAsync(object e);

    /// <summary>
    /// Loads the aggregate's state from the event stream using the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the aggregate to load.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task LoadAsync(TId id);

    /// <summary>
    /// Saves the current state of the aggregate, committing any uncommitted events.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveAsync();
}
