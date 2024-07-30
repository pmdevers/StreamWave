namespace StreamWave;

public interface IAggregateBuilder<TState, TId>
{
    IAggregateBuilder<TState, TId> WithEvents(Event[] events);
    IAggregateBuilder<TState, TId> WithLoader(Func<IServiceProvider, LoadEventStreamDelegate<TId>> loader);
    IAggregateBuilder<TState, TId> WithSaver(Func<IServiceProvider, SaveAggregateDelegate<TState, TId>> saver);
    IAggregateBuilder<TState, TId> WithApplier(Func<IServiceProvider, ApplyEventDelegate<TState>> applier);
    IAggregateBuilder<TState, TId> WithApplier<TEvent>(Func<TState, TEvent, Task<TState>> applier)
        where TEvent : Event;
    IAggregateBuilder<TState, TId> WithValidator(Func<IServiceProvider, ValidateStateDelegate<TState>> validator);
    IAggregateBuilder<TState, TId> WithValidator(Func<TState, bool> rule, string message);
}
