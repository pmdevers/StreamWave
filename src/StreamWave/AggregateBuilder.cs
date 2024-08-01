using StreamWave.Extensions;

namespace StreamWave;

internal class AggregateBuilder<TState, TId> : IAggregateBuilder<TState, TId>
    where TState : IAggregateState<TId>
{
    private readonly Dictionary<Type, ApplyEventDelegate<TState>> _events = [];
    private readonly List<ValidationRule<TState>> _rules = [];
    private readonly CreateStateDelegate<TState, TId> _creator;

    private Func<IServiceProvider, ValidateStateDelegate<TState>>? _validator;
    private Func<IServiceProvider, ApplyEventDelegate<TState>>? _applier = null;
    private Func<IServiceProvider, LoadEventStreamDelegate<TId>>? _loader = null;
    private Func<IServiceProvider, SaveAggregateDelegate<TState, TId>>? _saver = null;

    internal AggregateBuilder(CreateStateDelegate<TState, TId> creator)
    {
        _creator = creator;
    }
    public IAggregateBuilder<TState, TId> WithEvents(EventRecord[] events)
    {
        _loader = (_) => AggregateBuilderDefaults.DefaultLoader<TId>(events);
        return this;
    }

    public IAggregate<TState, TId> Build(IServiceProvider serviceProvider)
    {
        var applier = _applier?.Invoke(serviceProvider) ?? AggregateBuilderDefaults.DefaultApplier(_events);
        var validator = _validator?.Invoke(serviceProvider) ?? AggregateBuilderDefaults.DefaultValidator(_rules);
        var loader = _loader?.Invoke(serviceProvider) ?? AggregateBuilderDefaults.DefaultLoader<TId>();
        var saver = _saver?.Invoke(serviceProvider) ?? AggregateBuilderDefaults.DefaultSaver<TState, TId>();

        return new Aggregate<TState, TId>(_creator, applier, validator, loader, saver);
    }

    public IAggregateBuilder<TState, TId> WithSaver(Func<IServiceProvider, SaveAggregateDelegate<TState, TId>> saver)
    {
        _saver = saver;
        return this;
    }

    public IAggregateBuilder<TState, TId> WithStreamLoader(Func<IServiceProvider, LoadEventStreamDelegate<TId>> loader)
    {
        _loader = loader;
        return this;
    }

    public IAggregateBuilder<TState, TId> WithValidator(Func<IServiceProvider, ValidateStateDelegate<TState>> validator)
    {
        _validator = validator;
        return this;
    }

    public IAggregateBuilder<TState, TId> WithValidator(Func<TState, bool> rule, string message)
    {
        _rules.Add(new(rule, message));
        return this;
    }

    public IAggregateBuilder<TState, TId> WithApplier(Func<IServiceProvider, ApplyEventDelegate<TState>> applier)
    {
        _applier = applier;
        return this;
    }

    public IAggregateBuilder<TState, TId> WithApplier<TEvent>(Func<TState, TEvent, TState> applier) where TEvent : notnull
    {
        _events.Add(typeof(TEvent), (state, e) => applier(state, (TEvent)e));
        return this;
    }
}
