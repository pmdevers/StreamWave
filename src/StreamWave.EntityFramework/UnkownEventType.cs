namespace StreamWave.EntityFramework;

/// <summary>
/// Represents an event with an unknown type in an event-sourced system.
/// This event is used when the specific type of the event cannot be determined or is not recognized.
/// </summary>
/// <param name="EventName">The name of the event type, which is not recognized or registered in the system.</param>
/// <param name="Payload">The serialized data associated with the unknown event.</param>
public record UnknownEventType(string EventName, byte[] Payload) : Event;
