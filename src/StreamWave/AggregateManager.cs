using StreamWave.Extensions;

namespace StreamWave;

internal class AggregateMangerOptions<TState, TId>(CreateStateDelegate<TState, TId> creator)
{
    public Func<IServiceProvider, CreateStateDelegate<TState, TId>> Creator { get; set; } = (_) => creator;
    public Func<IServiceProvider, ApplyEventDelegate<TState>> Applier { get; set; } = (_) => AggregateBuilderDefaults.DefaultApplier<TState>();
    public Func<IServiceProvider, ValidateStateDelegate<TState>> Validator { get; set; } = (_) => AggregateBuilderDefaults.DefaultValidator<TState>();
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
        return Create(id, state, EventStream.Create());
    }

    public async Task<IAggregate<TState, TId>> LoadAsync(TId id)
    {
        var state = creator(id);
        var stream = await loader(id) ?? EventStream.Create();
        return Create(id, state, stream);
    }

    public async Task SaveAsync(IAggregate<TState, TId> aggregate)
    {
        await saver(aggregate);
    }

    private Aggregate<TState, TId> Create(TId id, TState state, EventStream stream)
        => new(id, state, stream, applier, validator);
}
