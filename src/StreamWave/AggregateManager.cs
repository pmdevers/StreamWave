using StreamWave.Extensions;

namespace StreamWave;

internal class AggregateManager<TState, TId>(
    AggregateBuilder<TState, TId> builder, IServiceProvider serviceProvider) : IAggregateManager<TState, TId>
    where TState : IAggregateState<TId>
{
    public IAggregate<TState, TId> Create()
    {
        var aggregate = builder.Build(serviceProvider);
        return aggregate;
    }

    public async Task<IAggregate<TState, TId>> LoadAsync(TId id)
    {
        var aggregate = builder.Build(serviceProvider);
        await aggregate.LoadAsync(id);
        return aggregate;
    }

    public async Task SaveAsync(IAggregate<TState, TId> aggregate)
    {
        if(aggregate is Aggregate<TState, TId> a)
        {
            await a.SaveAsync();
        }
    }
}
