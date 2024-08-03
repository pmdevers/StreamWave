using StreamWave.Extensions;

namespace StreamWave;

internal class AggregateBuilder<TState, TId> : IAggregateBuilder<TState, TId>
{
    private readonly Dictionary<Type, ApplyEventDelegate<TState>> _events = [];
    private readonly List<ValidationRule<TState>> _rules = [];

    public AggregateMangerOptions<TState, TId> Options { get; set; }

    internal AggregateBuilder(CreateStateDelegate<TState, TId> creator)
    {
        Options = new AggregateMangerOptions<TState, TId>(creator);
    }
    public IAggregateBuilder<TState, TId> WithEvents(params object[] events)
    {
        Options.Loader = (_) => AggregateBuilderDefaults.DefaultLoader<TId>(events);
        return this;
    }

    public AggregateManager<TState, TId> Build(IServiceProvider serviceProvider)
    {
        var creator = Options.Creator(serviceProvider);
        var applier = Options.Applier(serviceProvider, _events);
        var validator = Options.Validator(serviceProvider, _rules);
        var loader = Options.Loader(serviceProvider);
        var saver = Options.Saver(serviceProvider);

        return new AggregateManager<TState, TId>(creator, applier, validator, loader, saver);
    }

    public IAggregateBuilder<TState, TId> WithSaver(Func<IServiceProvider, SaveAggregateDelegate<TState, TId>> saver)
    {
        Options.Saver = saver;
        return this;
    }

    public IAggregateBuilder<TState, TId> WithStreamLoader(Func<IServiceProvider, LoadEventStreamDelegate<TId>> loader)
    {
        Options.Loader = loader;
        return this;
    }

    public IAggregateBuilder<TState, TId> WithValidator(Func<IServiceProvider, List<ValidationRule<TState>>, ValidateStateDelegate<TState>> validator)
    {
        Options.Validator = validator;
        return this;
    }

    public IAggregateBuilder<TState, TId> WithValidator(Func<TState, bool> rule, string message)
    {
        _rules.Add(new(rule, message));
        return this;
    }

    public IAggregateBuilder<TState, TId> WithApplier(Func<IServiceProvider, Dictionary<Type, ApplyEventDelegate<TState>>, ApplyEventDelegate<TState>> applier)
    {
        Options.Applier = applier;
        return this;
    }

    public IAggregateBuilder<TState, TId> WithApplier<TEvent>(Func<TState, TEvent, TState> applier) where TEvent : notnull
    {
        _events.Add(typeof(TEvent), (state, e) => applier(state, (TEvent)e));
        return this;
    }
}
