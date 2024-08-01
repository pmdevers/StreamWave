using StreamWave.Extensions;

namespace StreamWave;

internal class AggregateManager<TState, TId>(
    IAggregateBuilder<TState, TId> builder, IServiceProvider serviceProvider) : IAggregateManager<TState, TId>
    where TState : IAggregateState<TId>
{
    public Task<IAggregate<TState, TId>> Create()
    {
        var aggregate = builder.Build(serviceProvider);
        return Task.FromResult(aggregate);
    }

    public async Task<IAggregate<TState, TId>> LoadAsync(TId id)
    {
        var aggregate = builder.Build(serviceProvider);
        await aggregate.LoadAsync(id);
        return aggregate;
    }

    public Task SaveAsync(IAggregate<TState, TId> aggregate)
    {
        return aggregate.SaveAsync();
    }
}
