namespace StreamWave;

/// <summary>
/// Used for getting DateTime.Now(), time is changeable for unit testing
/// </summary>
public static class SystemTime
{
    /// <summary> Normally this is a pass-through to DateTime.Now, but it can be overridden with SetDateTime( .. ) for testing or debugging.
    /// </summary>
#pragma warning disable S6354 // Use a testable date/time provider
    public static Func<DateTime> Now { get; private set; } = () => DateTime.Now;
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
