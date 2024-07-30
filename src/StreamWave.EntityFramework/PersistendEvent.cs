namespace StreamWave.EntityFramework;

/// <summary>
/// Represents a persisted event in an event-sourced system, including metadata and serialized payload.
/// </summary>
/// <typeparam name="TId">The type of the stream identifier associated with the event.</typeparam>
/// <param name="Id">The unique identifier of the persisted event.</param>
/// <param name="StreamId">The identifier of the event stream to which this event belongs.</param>
/// <param name="Version">The version of the event within the stream, indicating the order of occurrence.</param>
/// <param name="EventName">The name or type of the event, typically used for deserialization purposes.</param>
/// <param name="Payload">The serialized data of the event, containing the event details.</param>
public record PersistedEvent<TId>(Guid Id, TId StreamId, int Version, string EventName, byte[] Payload);
