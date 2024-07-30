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


/// <summary>
/// Interface for managing aggregates, including creation, loading, and saving operations.
/// </summary>
/// <typeparam name="TState">The type of the aggregate state.</typeparam>
/// <typeparam name="TId">The type of the identifier used for the aggregate.</typeparam>
public interface IAggregateManager<TState, TId>
{
    /// <summary>
    /// Creates a new aggregate with the initial state.
    /// </summary>
    /// <returns>A task representing the asynchronous operation, with the newly created aggregate as the result.</returns>
    Task<IAggregate<TState, TId>> Create();

    /// <summary>
    /// Loads an existing aggregate based on its identifier.
    /// </summary>
    /// <param name="id">The identifier of the aggregate to be loaded.</param>
    /// <returns>A task representing the asynchronous operation, with the loaded aggregate as the result.</returns>
    Task<IAggregate<TState, TId>> LoadAsync(TId id);

    /// <summary>
    /// Saves the current state of the specified aggregate.
    /// </summary>
    /// <param name="aggregate">The aggregate to be saved.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveAsync(IAggregate<TState, TId> aggregate);
}

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
