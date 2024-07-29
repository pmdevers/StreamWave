using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace StreamWave.EntityFramework;
public static class AggregateBuilderExtensions
{
    public static IAggregateBuilder<TState, TKey> WithEntityFramework<TContext, TState, TKey>(this IAggregateBuilder<TState, TKey> builder)
        where TContext : DbContext
        where TState : class
        where TKey : struct
    {
            builder.WithLoader((s) =>
                  (id) =>
                  {
                      var context = s.GetRequiredService<TContext>();
                      return AggregateStore<TState, TKey>.LoadAsync(context, id);
                  }
            )
            .WithSaver((s) =>
                (a) => {
                    var context = s.GetRequiredService<TContext>();
                    return AggregateStore<TState, TKey>.SaveAsync(context, a);
                });
        return builder;
    }
}
