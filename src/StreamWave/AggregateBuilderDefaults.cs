namespace StreamWave;

internal static class AggregateBuilderDefaults
{
    public static ApplyEventDelegate<TState> DefaultApplier<TState>(Dictionary<Type, ApplyEventDelegate<TState>> events)
        => (state, e) => events.TryGetValue(e.GetType(), out var applier)
            ? applier(state, e)
            : state;

    public static ApplyEventDelegate<TState> DefaultApplier<TState>()
        => (state, _) => state;

    public static ValidateStateDelegate<TState> DefaultValidator<TState>(List<ValidationRule<TState>> rules) =>
        (state) => rules.Where(x => x.Rule(state))
                         .Select(x => new ValidationMessage(x.Message))
                         .ToArray();

    public static ValidateStateDelegate<TState> DefaultValidator<TState>() =>
       (_) => [];

    public static LoadEventStreamDelegate<TId> DefaultLoader<TId>(IEnumerable<EventRecord>? events = null)
        => (_) => Task.FromResult(EventStream.Create(events));
    
    public static SaveAggregateDelegate<TState, TId> DefaultSaver<TState, TId>()
        => (aggregate) => {
            var events = aggregate.Stream.GetUncommittedEvents();
            var stream = EventStream.Create([.. events, .. aggregate.Stream.GetUncommittedEvents() ]);
            return Task.FromResult(stream);
        };
}


/// <summary>
/// Represents a validation rule for a specific state type in an aggregate.
/// </summary>
/// <typeparam name="TState">The type of the state to which the validation rule applies.</typeparam>
/// <param name="Rule">A function that defines the validation logic for the state. 
/// It returns a boolean indicating whether the state is valid.</param>
/// <param name="Message">The message to be displayed if the validation fails.</param>
public record ValidationRule<TState>(Func<TState, bool> Rule, string Message);
