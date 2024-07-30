namespace StreamWave;

public delegate TState CreateStateDelegate<out TState, in TId>() 
    where TState : IAggregateState<TId>;
public delegate Task<TState> ApplyEventDelegate<TState>(TState state, Event e);
public delegate Task<IEventStream<TId>?> LoadEventStreamDelegate<TId>(TId id);
public delegate Task<IEventStream<TId>> SaveAggregateDelegate<TState, TId>(IAggregate<TState, TId> aggregate);

public delegate ValidationMessage[] ValidateStateDelegate<in TState>(TState state);

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
    public TState State { get; private set; }
    public ValidationMessage[] Messages => _validator(State);
    public bool IsValid => Messages.Length == 0;
    public IEventStream<TId> Stream => _stream;

    public async Task ApplyAsync(Event e)
    {
        State = await _applier(State, e);
        _stream.Append(e);
    }
    public async Task LoadAsync(TId id)
    {
        _stream = await _loader(id) ?? EventStream.Create(id);
        await UpdateState();
    }
    public async Task SaveAsync()
    {
        _stream = await _saver(this);
        await UpdateState();
    }
    private async Task UpdateState()
    {
        State = await _stream.AggregateAsync(_creator(), async (state, e) => await _applier(state, e));
        State.Id = _stream.Id;
    }
}


