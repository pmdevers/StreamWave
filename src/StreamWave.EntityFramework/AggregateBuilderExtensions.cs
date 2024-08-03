using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using System.Reflection.Emit;

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

    /// <summary>
    /// Adds an aggregate to the model builder with the specified state and key.
    /// </summary>
    /// <typeparam name="TState">The type of the state entity.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <param name="builder">The model builder to add the aggregate to.</param>
    /// <param name="hasKey">An expression that specifies the key for the state entity.</param>
    /// <returns>The modified model builder.</returns>
    public static EntityTypeBuilder<TState> AddAggregate<TState, TKey>(this ModelBuilder builder, Expression<Func<TState, object?>> hasKey)
        where TState : class
    {
        var entityBuilder = builder.Entity<TState>();
        
        entityBuilder.HasKey(hasKey);

        builder.ApplyConfiguration(new EventstreamConfiguration<TState, TKey>());

        return entityBuilder;
    }
}
