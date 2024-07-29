namespace StreamWave;

public interface IAggregate<out TState, TId>
{
    TState State { get; }
    IEventStream<TId> Stream { get; }
    ValidationMessage[] Messages { get; }
    bool IsValid { get; }
    void Apply(Event e);
    Task LoadAsync(TId id);
    Task SaveAsync();
}
