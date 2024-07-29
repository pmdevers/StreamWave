namespace StreamWave;

public interface IAggregateBuilder<TState>
{
    IAggregateBuilder<TState> WithEvents(Event[] events);
    IAggregateBuilder<TState> WithLoader(Func<IServiceProvider, LoadEventStreamDelegate> loader);
    IAggregateBuilder<TState> WithSaver(Func<IServiceProvider, SaveAggregateDelegate<TState>> saver);
    IAggregateBuilder<TState> WithApplier(Func<IServiceProvider, ApplyEventDelegate<TState>> applier);
    IAggregateBuilder<TState> WithApplier<TEvent>(Func<TState, TEvent, TState> applier)
        where TEvent : Event;
    IAggregateBuilder<TState> WithValidator(Func<IServiceProvider, ValidateStateDelegate<TState>> validator);
    IAggregateBuilder<TState> WithValidator(Func<TState, bool> rule, string message);
}
