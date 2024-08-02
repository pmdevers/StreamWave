using Microsoft.Extensions.DependencyInjection;

namespace StreamWave.Extensions;


/// <summary>
/// 
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TId"></typeparam>
    /// <param name="services"></param>
    /// <param name="initialState"></param>
    /// <returns></returns>
    public static IAggregateBuilder<TState, TId> AddAggregate<TState, TId>(this IServiceCollection services, CreateStateDelegate<TState, TId> initialState)
    {
        var builder = new AggregateBuilder<TState, TId>(initialState);
        services.AddSingleton(builder);
        services.AddScoped<IAggregateManager<TState, TId>, AggregateManager<TState, TId>>();
        return builder;
    }
}
