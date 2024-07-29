using System.IO;

namespace StreamWave;

public interface IStreamManager
{
    Task<IEventStream?> LoadAsync(Guid streamId);
    Task<IEventStream> SaveAsync(IEventStream stream);
}

public class StreamManager(Guid streamId, Event[] events) : IStreamManager
{
    private readonly Guid _streamId = streamId;
    private readonly Event[] _events = events;

    public async Task<IEventStream?> LoadAsync(Guid streamId)
    {
        if (streamId != _streamId)
            return null;

        await Task.CompletedTask;

        return EventStream.Create(streamId, _events);
    }

    public async Task<IEventStream> SaveAsync(IEventStream stream)
    {
        await Task.CompletedTask;
        return EventStream.Create(stream.Id, stream.GetUncommittedEvents());
    }
}
