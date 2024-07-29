using Microsoft.Extensions.DependencyInjection;

namespace StreamWave.Tests;

public class RegistrationTest
{
    [Fact]
    public void AddAggregate_Test()
    {
        var services = new ServiceCollection();

        services.AddAggregate<TestState, Guid>(() => new TestState {  Id = Guid.NewGuid() })
            .WithApplier<CreatedEvent>((state, e) => {
                state.Id = e.Id;
                return state;
            });

        var provider = services.BuildServiceProvider();

        var aggregate = provider.GetRequiredService<IAggregate<TestState, Guid>>();

        aggregate.Should().NotBeNull();
        aggregate.Stream.Id.Should().Be(aggregate.State.Id);
    }
}

public record CreatedEvent(Guid Id) : Event;
