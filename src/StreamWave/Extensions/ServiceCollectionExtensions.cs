using Microsoft.Extensions.DependencyInjection;

namespace StreamWave.Extensions;
public static class ServiceCollectionExtensions
{
    public static IAggregateBuilder<TState, TId> AddAggregate<TState, TId>(this IServiceCollection services, CreateStateDelegate<TState, TId> initialState)
        where TState : IAggregateState<TId>
    {
        var builder = new AggregateBuilder<TState, TId>(initialState);
        services.AddSingleton(builder);
        services.AddScoped<IAggregateManager<TState, TId>, AggregateManager<TState, TId>>();
        return builder;
    }
}

public interface IAggregateState<TId>
{
    TId Id { get; set; }
}

public interface IAggregateManager<TState, TId>
{
    Task<IAggregate<TState, TId>> Create();
    Task<IAggregate<TState, TId>> LoadAsync(TId id);
    Task SaveAsync(IAggregate<TState, TId> aggregate);
}

public class AggregateManager<TState, TId>(
    AggregateBuilder<TState, TId> builder, IServiceProvider serviceProvider) : IAggregateManager<TState, TId>
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
