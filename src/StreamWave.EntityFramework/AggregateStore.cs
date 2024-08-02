using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;
using System.Text.Json;

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

/// <summary>
/// 
/// </summary>
/// <param name="options"></param>
public class DefaultSerializer(JsonSerializerOptions options) : IEventSerializer
{
    /// <summary>
    /// 
    /// </summary>
    public JsonSerializerOptions Options { get; } = options;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public object? Deserialize(byte[] data, Type type)
    {
        using var stream = new MemoryStream(data);
        return JsonSerializer.Deserialize(stream, type, Options);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="type"></param>
    /// <returns></returns>
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

    public async Task<IAsyncEnumerable<object>> SaveAsync(IAggregate<TState, TId> aggregate)
    {
        SaveState(aggregate);
        SaveEvents(aggregate);

        await context.SaveChangesAsync();

        return await LoadStreamAsync(aggregate.Id);
    }

    public Task<IAsyncEnumerable<object>> LoadStreamAsync(TId id)
    {
        var events = context.Set<PersistedEvent<TId>>()
            .Where(x => x.StreamId.Equals(id))
            .OrderBy(x => x.Version)
            .Select(x => GetEvent(x))
            .AsAsyncEnumerable();

        return Task.FromResult(events);
    }

    private object GetEvent(PersistedEvent<TId> x)
    {
        var eventType = Type.GetType(x.EventName);

        if (eventType is not null)
        {
            var e = _serializer.Deserialize(x.Payload, eventType);
            if (e is not null)
            {
                return e;
            }
        }

        return new UnknownEventType(x.EventName, x.Payload);
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
            var type = e.GetType();
            context.Add(new PersistedEvent<TId>(
               Guid.NewGuid(),
               aggregate.Id,
               version,
               type.AssemblyQualifiedName ?? string.Empty,
               _serializer.Serialize(e, type)
            ));
        }
    }
}
