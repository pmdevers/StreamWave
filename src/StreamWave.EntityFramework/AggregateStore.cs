using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace StreamWave.EntityFramework;

public static partial class AggregateStore<TState, TId>
    where TState : class
    where TId : struct
{
    public static async Task<IEventStream<TId>> SaveAsync(DbContext context, IAggregate<TState, TId> aggregate)
    {
        if (!aggregate.Stream.HasUncommittedChanges)
        {
            return aggregate.Stream;
        }

        SaveState(context, aggregate.State);
        SaveEvents(context, aggregate.Stream);

        await context.SaveChangesAsync();

        return aggregate.Stream.Commit();
    }

    private static void SaveState(DbContext context, TState state)
    {
        context.Remove(state);
        context.Add(state);
    }

    private static void SaveEvents(DbContext context, IEventStream<TId> stream)
    {
        var events = stream.GetUncommittedEvents();

        var version = stream.Version;

        foreach (var e in events)
        {
            version++;
            context.Add(new PersistendEvent<TId>(
               Guid.NewGuid(),
               stream.Id,
               version,
               e.GetType().AssemblyQualifiedName ?? string.Empty,
               JsonSerializer.Serialize(e, e.GetType(), JsonSerializerOptions.Default)
           ));
        }
    }

    public static async Task<IEventStream<TId>?> LoadAsync(DbContext context, TId id)
    {
        var events = await context.Set<PersistendEvent<TId>>()
            .Where(x => x.StreamId.Equals(id))
            .OrderBy(x => x.Version)
            .Select(x => GetEvent(x))
            .ToArrayAsync();

        return EventStream.Create(id, events);
    }

    private static Event GetEvent(PersistendEvent<TId> x)
    {
        var eventType = Type.GetType(x.EventName);

        if (eventType == null)
        {
            return new UnkownEventType(x.EventName, x.Payload);
        }

        var e = JsonSerializer.Deserialize(x.Payload, eventType);
        if (e is Event ev)
        {
            return ev;
        }

        return new UnkownEventType(x.EventName, x.Payload);
    }
}
