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
                      var serializer = s.GetRequiredService<IEventSerializer>();
                      return new AggregateStore<TState, TKey>(context, serializer).LoadAsync(id);
                  }
            )
            .WithSaver((s) =>
                (a) => {
                    var context = s.GetRequiredService<TContext>();
                    var serializer = s.GetRequiredService<IEventSerializer>();
                    return new AggregateStore<TState, TKey>(context, serializer).SaveAsync(a);
                });
        return builder;
    }
}
