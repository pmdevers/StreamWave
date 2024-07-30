using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StreamWave.EntityFramework;

public interface IEventSerializer
{
    object? Deserialize(byte[] data, Type type);
    byte[] Serialize(object value, Type type);
}

public class DefaultSerializer(JsonSerializerOptions options) : IEventSerializer
{
    public JsonSerializerOptions Options { get; } = options;

    public object? Deserialize(byte[] data, Type type)
    {
        using var stream = new MemoryStream(data);
        return JsonSerializer.Deserialize(stream, type, Options);
    }

    public byte[] Serialize(object value, Type type)
    {
        using var stream = new MemoryStream();
        JsonSerializer.Serialize(stream, value, type, Options);
        stream.Flush();
        return stream.ToArray();
    }
}

public class AggregateStore<TState, TId>(DbContext context, IEventSerializer serializer)
    where TState : class
    where TId : struct
{
    private readonly IEventSerializer _serializer = serializer;

    public async Task<IEventStream<TId>> SaveAsync(IAggregate<TState, TId> aggregate)
    {
        if (!aggregate.Stream.HasUncommittedChanges)
        {
            return aggregate.Stream;
        }

        SaveState(aggregate.State);
        SaveEvents(aggregate.Stream);

        await context.SaveChangesAsync();

        return aggregate.Stream.Commit();
    }

    private void SaveState(TState state)
    {
        context.Remove(state);
        context.Add(state);
    }

    private void SaveEvents(IEventStream<TId> stream)
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
               _serializer.Serialize(e, e.GetType())
           ));
        }
    }

    public async Task<IEventStream<TId>?> LoadAsync(TId id)
    {
        var events = await context.Set<PersistendEvent<TId>>()
            .Where(x => x.StreamId.Equals(id))
            .OrderBy(x => x.Version)
            .Select(x => GetEvent(x))
            .ToArrayAsync();

        return EventStream.Create(id, events);
    }

    private Event GetEvent(PersistendEvent<TId> x)
    {
        var eventType = Type.GetType(x.EventName);

        if (eventType == null)
        {
            return new UnkownEventType(x.EventName, x.Payload);
        }

        var e = _serializer.Deserialize(x.Payload, eventType);
        if (e is Event ev)
        {
            return ev;
        }

        return new UnkownEventType(x.EventName, x.Payload);
    }
}
