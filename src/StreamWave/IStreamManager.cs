using System.IO;

namespace StreamWave;

public interface IStreamManager<TId>
{
    Task<IEventStream<TId>?> LoadAsync(TId streamId);
    Task<IEventStream<TId>> SaveAsync(IEventStream<TId> stream);
}

public class StreamManager<TId>(TId streamId, Event[] events) : IStreamManager<TId>
{
    private readonly TId _streamId = streamId;
    private readonly Event[] _events = events;

    public async Task<IEventStream<TId>?> LoadAsync(TId streamId)
    {
        if (streamId?.Equals(_streamId) == true)
            return null;

        await Task.CompletedTask;

        return EventStream.Create(streamId, _events);
    }

    public async Task<IEventStream<TId>> SaveAsync(IEventStream<TId> stream)
    {
        await Task.CompletedTask;
        return EventStream.Create(stream.Id, stream.GetUncommittedEvents());
    }
}
