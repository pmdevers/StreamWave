using Microsoft.Extensions.DependencyInjection;

namespace StreamWave;
public static class ServiceCollectionExtensions
{
    public static IAggregateBuilder<TState, TId> AddAggregate<TState, TId>(this IServiceCollection services, CreateStateDelegate<TState, TId> initialState)
        where TState : IAggregateState<TId>
    {
        var builder = new AggregateBuilder<TState, TId>(initialState);
        services.AddSingleton(builder);
        services.AddTransient(x => x.GetRequiredService<AggregateBuilder<TState, TId>>().Build(x));
        return builder;
    }
}

public interface IAggregateState<TId>
{
    TId Id { get; set; }
}
