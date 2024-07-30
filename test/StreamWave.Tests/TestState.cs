using StreamWave.Extensions;

namespace StreamWave.Tests;

public class TestState : IAggregateState<Guid>
{
    public Guid Id { get; set; }
    public string Property { get; set; } = string.Empty;
    
}
