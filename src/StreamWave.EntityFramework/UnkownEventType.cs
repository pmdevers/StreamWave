namespace StreamWave.EntityFramework;

public record UnkownEventType(string EventName, byte[] Payload) : Event;
