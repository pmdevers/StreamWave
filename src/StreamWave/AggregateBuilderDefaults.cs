namespace StreamWave;

internal static class AggregateBuilderDefaults
{
    public static ApplyEventDelegate<TState> DefaultApplier<TState>(Dictionary<Type, ApplyEventDelegate<TState>> events)
        => (state, e) => events.TryGetValue(e.GetType(), out var applier)
            ? applier(state, e)
            : state;

    public static ValidateStateDelegate<TState> DefaultValidator<TState>(List<ValidationRule<TState>> rules) =>
        (state) => rules.Where(x => x.Rule(state))
                         .Select(x => new ValidationMessage(x.Message))
                         .ToArray();

    public static LoadEventStreamDelegate DefaultLoader(Event[]? events = null)
        => (streamId) => Task.FromResult(events is not null ? EventStream.Create(streamId, events) : null);

    public static SaveAggregateDelegate<TState> DefaultSaver<TState>()
        => (aggregate) => Task.FromResult(EventStream.Create(aggregate.Stream.Id, aggregate.Stream.GetUncommittedEvents()));
}
public record ValidationRule<TState>(Func<TState, bool> Rule, string Message);
