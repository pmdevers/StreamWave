namespace StreamWave;

public interface IAggregate<out TState, TId>
{
    TState State { get; }
    IEventStream<TId> Stream { get; }
    ValidationMessage[] Messages { get; }
    bool IsValid { get; }
    Task ApplyAsync(Event e);
    Task LoadAsync(TId id);
    Task SaveAsync();
}
