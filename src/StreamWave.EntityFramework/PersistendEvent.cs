namespace StreamWave.EntityFramework;

public record PersistendEvent<TId>(Guid Id, TId StreamId, int Version, string EventName, byte[] Payload);
