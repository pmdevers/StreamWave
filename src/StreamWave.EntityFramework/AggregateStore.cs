using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StreamWave.EntityFramework;

/// <summary>
/// Defines methods for serializing and deserializing event data in an event-sourced system.
/// </summary>
public interface IEventSerializer
{
    /// <summary>
    /// Deserializes the provided byte array into an object of the specified type.
    /// </summary>
    /// <param name="data">The byte array containing the serialized data.</param>
    /// <param name="type">The type of the object to deserialize.</param>
    /// <returns>The deserialized object, or null if the data cannot be deserialized.</returns>
    object? Deserialize(byte[] data, Type type);

    /// <summary>
    /// Serializes the provided object into a byte array.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="type">The type of the object being serialized.</param>
    /// <returns>A byte array containing the serialized data.</returns>
    byte[] Serialize(object value, Type type);
}

internal class DefaultSerializer(JsonSerializerOptions options) : IEventSerializer
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

internal class Store<TState, TId>(DbContext context, IEventSerializer serializer)
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
            context.Add(new PersistedEvent<TId>(
               Guid.NewGuid(),
               stream.Id,
               version,
               e.EventType.AssemblyQualifiedName ?? string.Empty,
               _serializer.Serialize(e.Event, e.EventType),
               e.OccurredOn
            ));
        }
    }

    public IEventStream<TId> LoadStreamAsync(TId id)
    {
        var events = context.Set<PersistedEvent<TId>>()
            .Where(x => x.StreamId.Equals(id))
            .OrderBy(x => x.Version)
            .Select(x => GetEvent(x));

        return EventStream.Create(id, events);
    }

    private EventRecord GetEvent(PersistedEvent<TId> x)
    {
        var eventType = Type.GetType(x.EventName);

        if (eventType is not null)
        {
            var e = _serializer.Deserialize(x.Payload, eventType);
            if(e is not null)
            {
                return new(e, eventType, x.OccurredOn);
            }
        }

        return new(new UnknownEventType(x.EventName, x.Payload), typeof(UnknownEventType), x.OccurredOn);
    }
}
