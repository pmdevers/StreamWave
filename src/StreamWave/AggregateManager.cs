namespace StreamWave;

internal class AggregateMangerOptions<TState, TId>(CreateStateDelegate<TState, TId> creator)
{
    public Func<IServiceProvider, CreateStateDelegate<TState, TId>> Creator { get; set; } = (_) => creator;
    public Func<IServiceProvider, Dictionary<Type, ApplyEventDelegate<TState>>, ApplyEventDelegate<TState>> Applier { get; set; } = (_, eventHandlers) => AggregateBuilderDefaults.DefaultApplier(eventHandlers);
    public Func<IServiceProvider, List<ValidationRule<TState>>, ValidateStateDelegate<TState>> Validator { get; set; } = (_, validationRules) => AggregateBuilderDefaults.DefaultValidator(validationRules);
    public Func<IServiceProvider, LoadEventStreamDelegate<TId>> Loader { get; set; } = (_) => AggregateBuilderDefaults.DefaultLoader<TId>();
    public Func<IServiceProvider, SaveAggregateDelegate<TState, TId>> Saver { get; set; } = (_) => AggregateBuilderDefaults.DefaultSaver<TState, TId>();
}

internal class AggregateManager<TState, TId>(
    CreateStateDelegate<TState, TId> creator,
    ApplyEventDelegate<TState> applier,
    ValidateStateDelegate<TState> validator,
    LoadEventStreamDelegate<TId> loader,
    SaveAggregateDelegate<TState, TId> saver
    ) : IAggregateManager<TState, TId>
{
    public IAggregate<TState, TId> Create(TId id)
    {
        var state = creator(id);
        var empty = new List<EventData>().ToAsyncEnumerable();
        return Create(id, state, empty);
    }

    public async Task<IAggregate<TState, TId>> LoadAsync(TId id)
    {
        var state = creator(id);
        var stream = await loader(id);
        return Create(id, state, stream);
    }

    public async Task<IAggregate<TState, TId>> SaveAsync(IAggregate<TState, TId> aggregate)
    {
        if (aggregate.ExpectedVersion == aggregate.Version)
        {
            return aggregate;
        }

        var state = creator(aggregate.Id);
        var stream = await saver(aggregate);
        return Create(aggregate.Id, state, stream);
    }

    private Aggregate<TState, TId> Create(TId id, TState state, IAsyncEnumerable<EventData> stream)
        => new(id, state, stream, applier, validator);
}
