using System.Diagnostics.CodeAnalysis;

namespace StreamWave;

/// <summary>
/// Used for getting DateTime.Now(), time is changeable for unit testing
/// </summary>
[SuppressMessage("Major Code Smell", "S6354:Use a testable date/time provider", Justification = "this is the testable date/time provider!")]
public static class SystemTime
{
    /// <summary> Normally this is a pass-through to DateTime.Now, but it can be overridden with SetDateTime( .. ) for testing or debugging.
    /// </summary>
    public static Func<DateTime> Now { get; private set; } = () => DateTime.Now;
    
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
        Now = () => DateTime.Now;
    }
}
