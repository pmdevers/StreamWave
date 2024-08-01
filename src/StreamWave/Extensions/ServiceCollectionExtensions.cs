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
        where TState : IAggregateState<TId>
    {
        var builder = new AggregateBuilder<TState, TId>(initialState);
        services.AddSingleton(builder);
        services.AddScoped<IAggregateManager<TState, TId>, AggregateManager<TState, TId>>();
        return builder;
    }
}

/// <summary>
/// Interface for indication a class that it is an AggregateState.
/// </summary>
/// <typeparam name="TId"></typeparam>
public interface IAggregateState<TId>
{

    /// <summary>
    /// The Identifier of the class.
    /// </summary>
    TId Id { get; set; }
}
