namespace StreamWave;

public static class AggregateBuilder
{
    public static AggregateBuilder<TState> Create<TState>(TState initialState)
        => Create(() => initialState);

    public static AggregateBuilder<TState> Create<TState>(CreateStateDelegate<TState> creator)
        => new(creator);
}

public class AggregateBuilder<TState> : IAggregateBuilder<TState>
{
    private readonly Dictionary<Type, ApplyEventDelegate<TState>> _events = [];
    private readonly List<ValidationRule<TState>> _rules = [];
    private readonly CreateStateDelegate<TState> _creator;

    private Func<IServiceProvider, ValidateStateDelegate<TState>>? _validator;
    private Func<IServiceProvider, ApplyEventDelegate<TState>>? _applier = null;
    private Func<IServiceProvider, LoadEventStreamDelegate>? _loader = null;
    private Func<IServiceProvider, SaveAggregateDelegate<TState>>? _saver = null;

    internal AggregateBuilder(CreateStateDelegate<TState> creator)
    {
        _creator = creator;
    }
    public IAggregateBuilder<TState> WithEvents(Event[] events)
    {
        _loader = (_) => AggregateBuilderDefaults.DefaultLoader(events);
        return this;
    }

    public IAggregate<TState> Build(IServiceProvider serviceProvider)
    {
        var applier = _applier?.Invoke(serviceProvider) ?? AggregateBuilderDefaults.DefaultApplier(_events);
        var validator = _validator?.Invoke(serviceProvider) ?? AggregateBuilderDefaults.DefaultValidator(_rules);
        var loader = _loader?.Invoke(serviceProvider) ?? AggregateBuilderDefaults.DefaultLoader();
        var saver = _saver?.Invoke(serviceProvider) ?? AggregateBuilderDefaults.DefaultSaver<TState>();

        return new Aggregate<TState>(_creator, applier, validator, loader, saver);
    }

    public IAggregateBuilder<TState> WithSaver(Func<IServiceProvider, SaveAggregateDelegate<TState>> saver)
    {
        _saver = saver;
        return this;
    }

    public IAggregateBuilder<TState> WithLoader(Func<IServiceProvider, LoadEventStreamDelegate> loader)
    {
        _loader = loader;
        return this;
    }

    public IAggregateBuilder<TState> WithValidator(Func<IServiceProvider, ValidateStateDelegate<TState>> validator)
    {
        _validator = validator;
        return this;
    }

    public IAggregateBuilder<TState> WithValidator(Func<TState, bool> rule, string message)
    {
        _rules.Add(new(rule, message));
        return this;
    }

    public IAggregateBuilder<TState> WithApplier(Func<IServiceProvider, ApplyEventDelegate<TState>> applier)
    {
        _applier = applier;
        return this;
    }

    public IAggregateBuilder<TState> WithApplier<TEvent>(Func<TState, TEvent, TState> applier) where TEvent : Event
    {
        _events.Add(typeof(TEvent), (state, e) => applier(state, (TEvent)e));
        return this;
    }
}
