using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace StreamWave.EntityFramework;

/// <summary>
/// Provides extension methods for configuring aggregates using an Entity Framework context.
/// </summary>
public static class AggregateBuilderExtensions
{
    /// <summary>
    /// Configures the aggregate builder to use Entity Framework for loading and saving aggregates.
    /// </summary>
    /// <typeparam name="TContext">The type of the Entity Framework context.</typeparam>
    /// <typeparam name="TState">The type of the aggregate state.</typeparam>
    /// <typeparam name="TKey">The type of the identifier for the aggregate.</typeparam>
    /// <param name="builder">The aggregate builder to configure.</param>
    /// <returns>The configured <see cref="IAggregateBuilder{TState, TKey}"/> instance.</returns>
    /// <remarks>
    /// This method sets up the builder to use Entity Framework for the persistence of aggregate states.
    /// It registers loader and saver delegates that use a specified Entity Framework context and event serializer.
    /// </remarks>
    public static IAggregateBuilder<TState, TKey> WithEntityFramework<TContext, TState, TKey>(this IAggregateBuilder<TState, TKey> builder)
        where TContext : DbContext
        where TState : class
        where TKey : struct
    {
            builder.WithStreamLoader((s) =>
                  (id) =>
                  {
                      var context = s.GetRequiredService<TContext>();
                      var serializer = s.GetRequiredService<IEventSerializer>();
                      return new Store<TState, TKey>(context, serializer).LoadStreamAsync(id);
                  }
            )
            .WithSaver((s) =>
                (a) => {
                    var context = s.GetRequiredService<TContext>();
                    var serializer = s.GetRequiredService<IEventSerializer>();
                    return new Store<TState, TKey>(context, serializer).SaveAsync(a);
                });
        return builder;
    }
}
