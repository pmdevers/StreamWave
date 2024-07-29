namespace StreamWave;

public interface IEventStream : IReadOnlyCollection<Event>
{
    Guid Id { get; }
    int Version { get; }
    int ExpectedVersion { get; }
    DateTimeOffset? CreatedOn { get; }
    DateTimeOffset? LastModifiedOn { get; }
    void Append(Event e);
    Event[] GetUncommittedEvents();
    IEventStream Commit();
}

public abstract record Event
{
    public DateTimeOffset OccouredOn { get; set; } = SystemTime.Now();
}


/// <summary>
/// Used for getting DateTime.Now(), time is changeable for unit testing
/// </summary>
public static class SystemTime
{
    /// <summary> Normally this is a pass-through to DateTime.Now, but it can be overridden with SetDateTime( .. ) for testing or debugging.
    /// </summary>
#pragma warning disable S6354 // Use a testable date/time provider
#pragma warning disable CA2211 // Non-constant fields should not be visible
    public static Func<DateTime> Now { get; private set; } = () => DateTime.Now;
#pragma warning restore CA2211 // Non-constant fields should not be visible
#pragma warning restore S6354 // Use a testable date/time provider

    /// <summary> Set time to return when SystemTime.Now() is called.
    /// </summary>
    public static void SetDateTime(DateTime dateTimeNow)
    {
        Now = () => dateTimeNow;
    }

    /// <summary> Resets SystemTime.Now() to return DateTime.Now.
    /// </summary>
    public static void ResetDateTime()
    {
#pragma warning disable S6354 // Use a testable date/time provider
        Now = () => DateTime.Now;
#pragma warning restore S6354 // Use a testable date/time provider
    }
}
