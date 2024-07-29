namespace StreamWave.EntityFramework;

public static partial class AggregateStore<TState, TId>
    where TState : class
    where TId : struct
{
    public record UnkownEventType(string EventName, string Payload) : Event;
}
