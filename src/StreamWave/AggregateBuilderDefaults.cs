namespace StreamWave;

internal static class AggregateBuilderDefaults
{
    public static ApplyEventDelegate<TState> DefaultApplier<TState>(Dictionary<Type, ApplyEventDelegate<TState>> events)
        => async (state, e) => events.TryGetValue(e.GetType(), out var applier)
            ? await applier(state, e)
            : state;

    public static ValidateStateDelegate<TState> DefaultValidator<TState>(List<ValidationRule<TState>> rules) =>
        (state) => rules.Where(x => x.Rule(state))
                         .Select(x => new ValidationMessage(x.Message))
                         .ToArray();

    public static LoadEventStreamDelegate<TId> DefaultLoader<TId>(Event[]? events = null)
        => (streamId) => Task.FromResult(events is not null ? EventStream.Create(streamId, events) : null);

    public static SaveAggregateDelegate<TState, TId> DefaultSaver<TState, TId>()
        => (aggregate) => Task.FromResult(EventStream.Create(aggregate.Stream.Id, aggregate.Stream.GetUncommittedEvents()));
}


/// <summary>
/// Represents a validation rule for a specific state type in an aggregate.
/// </summary>
/// <typeparam name="TState">The type of the state to which the validation rule applies.</typeparam>
/// <param name="Rule">A function that defines the validation logic for the state. 
/// It returns a boolean indicating whether the state is valid.</param>
/// <param name="Message">The message to be displayed if the validation fails.</param>
public record ValidationRule<TState>(Func<TState, bool> Rule, string Message);
