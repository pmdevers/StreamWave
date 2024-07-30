using StreamWave.Extensions;

namespace StreamWave;

/// <summary>
/// Delegate for creating the initial state of an aggregate.
/// </summary>
/// <typeparam name="TState">The type of the aggregate state.</typeparam>
/// <typeparam name="TId">The type of the identifier used for the aggregate state.</typeparam>
/// <returns>The initial state of the aggregate.</returns>
public delegate TState CreateStateDelegate<out TState, in TId>()
    where TState : IAggregateState<TId>;

/// <summary>
/// Delegate for applying an event to an aggregate's state.
/// </summary>
/// <typeparam name="TState">The type of the aggregate state.</typeparam>
/// <param name="state">The current state of the aggregate.</param>
/// <param name="e">The event to be applied.</param>
/// <returns>A task representing the asynchronous operation, with the updated state as the result.</returns>
public delegate Task<TState> ApplyEventDelegate<TState>(TState state, Event e);

/// <summary>
/// Delegate for loading the event stream of an aggregate based on its identifier.
/// </summary>
/// <typeparam name="TId">The type of the identifier used for the aggregate.</typeparam>
/// <param name="id">The identifier of the aggregate.</param>
/// <returns>A task representing the asynchronous operation, with the loaded event stream as the result. 
/// The result can be null if the event stream does not exist.</returns>
public delegate Task<IEventStream<TId>?> LoadEventStreamDelegate<TId>(TId id);

/// <summary>
/// Delegate for saving the aggregate and returning the updated event stream.
/// </summary>
/// <typeparam name="TState">The type of the aggregate state.</typeparam>
/// <typeparam name="TId">The type of the identifier used for the aggregate.</typeparam>
/// <param name="aggregate">The aggregate to be saved.</param>
/// <returns>A task representing the asynchronous operation, with the updated event stream as the result.</returns>
public delegate Task<IEventStream<TId>> SaveAggregateDelegate<TState, TId>(IAggregate<TState, TId> aggregate);

/// <summary>
/// Delegate for validating the state of an aggregate.
/// </summary>
/// <typeparam name="TState">The type of the aggregate state.</typeparam>
/// <param name="state">The state to be validated.</param>
/// <returns>An array of validation messages indicating the validation results.</returns>
public delegate ValidationMessage[] ValidateStateDelegate<in TState>(TState state);

/// <summary>
/// Aggregate class representing an aggregate in a domain-driven design context.
/// </summary>
/// <typeparam name="TState"></typeparam>
/// <typeparam name="TId"></typeparam>
public class Aggregate<TState, TId>
    : IAggregate<TState, TId>
    where TState : IAggregateState<TId>
{
    private readonly CreateStateDelegate<TState, TId> _creator;
    private readonly ApplyEventDelegate<TState> _applier;
    private readonly ValidateStateDelegate<TState> _validator;
    private readonly LoadEventStreamDelegate<TId> _loader;
    private readonly SaveAggregateDelegate<TState, TId> _saver;

    internal Aggregate(
        CreateStateDelegate<TState, TId> creator,
        ApplyEventDelegate<TState> applier,
        ValidateStateDelegate<TState> validator,
        LoadEventStreamDelegate<TId> loader,
        SaveAggregateDelegate<TState, TId> saver)
    {
        _creator = creator;
        _applier = applier;
        _validator = validator;
        _loader = loader;
        _saver = saver;
        
        State = _creator();
        _stream = EventStream.Create(State.Id);
        State.Id = _stream.Id;
    }
    
    private IEventStream<TId> _stream;

    /// <summary>
    /// The current state of the aggregate.
    /// </summary>
    public TState State { get; private set; }

    /// <summary>
    /// Validation messages for the current state.
    /// </summary>
    public ValidationMessage[] Messages => _validator(State);

    /// <summary>
    /// Indicates whether the aggregate's state is valid.
    /// </summary>
    public bool IsValid => Messages.Length == 0;

    /// <summary>
    /// The event stream for external access.
    /// </summary>
    public IEventStream<TId> Stream => _stream;

    /// <summary>
    /// Applies an event to the aggregate, updating its state asynchronously.
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
    public async Task ApplyAsync(Event e)
    {
        State = await _applier(State, e);
        _stream.Append(e);
    }

    /// <summary>
    /// Loads the aggregate's state from the event stream based on its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task LoadAsync(TId id)
    {
        _stream = await _loader(id) ?? EventStream.Create(id);
        await UpdateState();
    }

    /// <summary>
    /// Saves the current state of the aggregate, updating the event stream.
    /// </summary>
    /// <returns></returns>
    public async Task SaveAsync()
    {
        _stream = await _saver(this);
        await UpdateState();
    }

    /// <summary>
    /// Updates the aggregate's state by applying events from the event stream.
    /// </summary>
    /// <returns></returns>
    private async Task UpdateState()
    {
        State = await _stream.AggregateAsync(_creator(), async (state, e) => await _applier(state, e));
        State.Id = _stream.Id;
    }
}


