using Microsoft.Extensions.DependencyInjection;

namespace StreamWave;
public static class ServiceCollectionExtensions
{
    public static IAggregateBuilder<TState> AddAggregate<TState>(this IServiceCollection services, TState initialState)
    {
        var builder = AggregateBuilder.Create(initialState);
        services.AddSingleton(builder);
        services.AddScoped(x => x.GetRequiredService<AggregateBuilder<TState>>().Build(x));
        return builder;
    }
}
