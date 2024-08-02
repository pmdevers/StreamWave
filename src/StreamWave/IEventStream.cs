namespace StreamWave;

/// <summary>
/// Represents an event stream, which is a sequence of events that captures changes to an aggregate's state.
/// </summary>
public interface IEventStream : IAsyncEnumerable<object>
{
    /// <summary>
    /// Gets a value indicating whether there are uncommitted changes in the event stream.
    /// </summary>
    bool HasUncommittedChanges { get; }

    /// <summary>
    /// Gets the current version of the event stream, which is the number of committed events.
    /// </summary>
    int Version { get; }

    /// <summary>
    /// Gets the expected version of the event stream, which includes both committed and uncommitted events.
    /// </summary>
    int ExpectedVersion { get; }

    /// <summary>
    /// Gets the date and time when the event stream was created, based on the first event's timestamp.
    /// </summary>
    DateTimeOffset? CreatedOn { get; }

    /// <summary>
    /// Gets the date and time when the event stream was last modified, based on the last event's timestamp.
    /// </summary>
    DateTimeOffset? LastModifiedOn { get; }

    /// <summary>
    /// Gets the array of uncommitted events, which are events that have been applied but not yet saved.
    /// </summary>
    /// <returns>An array of uncommitted events.</returns>
    EventRecord[] GetUncommittedEvents();

    /// <summary>
    /// Commits the uncommitted events to the event stream, returning a new event stream instance with these events.
    /// </summary>
    /// <returns>A new instance of <see cref="IEventStream"/> with the committed events.</returns>
    IEventStream Commit();
}
