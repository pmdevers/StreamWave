namespace StreamWave;

/// <summary>
/// Represents an abstract base class for domain events in an event-sourced system.
/// </summary>
public abstract record Event
{
    /// <summary>
    /// Gets or sets the timestamp indicating when the event occurred.
    /// </summary>
    public DateTimeOffset OccurredOn { get; set; } = SystemTime.Now();
}
