using Microsoft.EntityFrameworkCore;

namespace StreamWave.EntityFramework;

internal class Store<TState, TId>(DbContext context, IEventSerializer serializer)
    where TState : class
    where TId : struct
{
    private readonly IEventSerializer _serializer = serializer;

    public async Task<IAsyncEnumerable<EventData>> SaveAsync(IAggregate<TState, TId> aggregate)
    {
        SaveState(aggregate);
        SaveEvents(aggregate);

        await context.SaveChangesAsync();

        return await LoadStreamAsync(aggregate.Id);
    }

    public Task<IAsyncEnumerable<EventData>> LoadStreamAsync(TId id)
    {
        var events = context.Set<PersistedEvent<TId>>()
            .Where(x => x.StreamId.Equals(id))
            .OrderBy(x => x.Version)
            .Select(x => GetEvent(x))
            .AsAsyncEnumerable();

        return Task.FromResult(events);
    }

    private EventData GetEvent(PersistedEvent<TId> x)
    {
        var eventType = Type.GetType(x.EventName);

        if (eventType is not null)
        {
            var e = _serializer.Deserialize(x.Payload, eventType);
            if (e is not null)
            {
                return new(e, eventType, x.OccurredOn);
            }
        }

        return new(new UnknownEventType(x.EventName, x.Payload), typeof(UnknownEventType), x.OccurredOn);
    }
    private void SaveState(IAggregate<TState, TId> aggregate)
    {
        var entity = context.Find<TState>(aggregate.Id);
        

        if (entity != null) 
        {
            context.Entry(entity).State = EntityState.Detached;
            context.Update(aggregate.State);
            context.Entry(aggregate.State).State = EntityState.Modified;
        } else
        {
            context.Add(aggregate.State);
            context.Entry(aggregate.State).State = EntityState.Added;
        }
        
    }

    private void SaveEvents(IAggregate<TState, TId> aggregate)
    {
        var events = aggregate.GetUncommitedEvents();

        var version = aggregate.Version;

        foreach (var e in events)
        {
            version++;
            context.Add(new PersistedEvent<TId>(
               Guid.NewGuid(),
               aggregate.Id,
               version,
               e.EventType.AssemblyQualifiedName ?? string.Empty,
               _serializer.Serialize(e.Event, e.EventType),
               e.OccurredOn
            ));
        }
    }
}
