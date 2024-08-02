using StreamWave.Extensions;
using System.IO;
using System.Runtime.InteropServices;

namespace StreamWave;

/// <summary>
/// Delegate for creating the initial state of an aggregate.
/// </summary>
/// <typeparam name="TState">The type of the aggregate state.</typeparam>
/// <typeparam name="TId">The type of the identifier used for the aggregate state.</typeparam>
/// <returns>The initial state of the aggregate.</returns>
public delegate TState CreateStateDelegate<out TState, in TId>(TId id);

/// <summary>
/// Delegate for applying an event to an aggregate's state.
/// </summary>
/// <typeparam name="TState">The type of the aggregate state.</typeparam>
/// <param name="state">The current state of the aggregate.</param>
/// <param name="e">The event to be applied.</param>
/// <returns>A task representing the asynchronous operation, with the updated state as the result.</returns>
public delegate TState ApplyEventDelegate<TState>(TState state, object e);

/// <summary>
/// Delegate for loading the event stream of an aggregate based on its identifier.
/// </summary>
/// <typeparam name="TId">The type of the identifier used for the aggregate.</typeparam>
/// <param name="id">The identifier of the aggregate.</param>
/// <returns>A task representing the asynchronous operation, with the loaded event stream as the result. 
/// The result can be null if the event stream does not exist.</returns>
public delegate Task<IAsyncEnumerable<object>> LoadEventStreamDelegate<in TId>(TId id);

/// <summary>
/// Delegate for saving the aggregate and returning the updated event stream.
/// </summary>
/// <typeparam name="TState">The type of the aggregate state.</typeparam>
/// <typeparam name="TId">The type of the identifier used for the aggregate.</typeparam>
/// <param name="aggregate">The aggregate to be saved.</param>
/// <returns>A task representing the asynchronous operation, with the updated event stream as the result.</returns>
public delegate Task<IAsyncEnumerable<object>> SaveAggregateDelegate<TState, TId>(IAggregate<TState, TId> aggregate);

/// <summary>
/// Delegate for validating the state of an aggregate.
/// </summary>
/// <typeparam name="TState">The type of the aggregate state.</typeparam>
/// <param name="state">The state to be validated.</param>
/// <returns>An array of validation messages indicating the validation results.</returns>
public delegate ValidationMessage[] ValidateStateDelegate<in TState>(TState state);

internal class Aggregate<TState, TId>
    : IAggregate<TState, TId>
{
    private readonly ApplyEventDelegate<TState> _applier;
    private readonly ValidateStateDelegate<TState> _validator;
    private readonly IAsyncEnumerable<object> _events;
    private readonly List<object> _newEvents = [];
    private int _version;

    internal Aggregate(
        TId id,
        TState state,
        IAsyncEnumerable<object> events,
        ApplyEventDelegate<TState> applier,
        ValidateStateDelegate<TState> validator
    )
    {
        _applier = applier;
        _validator = validator;
        _events = events;

        Id = id;
        State = state;

        var runner = Task.Run(async () => await _events.AggregateAsync(State, (state, e) => 
        {
            _version++;
            return _applier(state, e);
        }));

        runner.Wait();
    }

    public TId Id { get; private set; }

    public TState State { get; private set; }

    public ValidationMessage[] Messages => _validator(State);

    public bool IsValid => Messages.Length == 0;

    public int Version => _version;

    public int ExpectedVersion => _version + _newEvents.Count;

    public Task ApplyAsync(object e)
    {
        _newEvents.Add(e);
        State = _applier(State, e);
        return Task.CompletedTask;
    }

    public IEnumerable<object> GetUncommitedEvents()
        => _newEvents;
}


