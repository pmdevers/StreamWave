using Microsoft.Extensions.DependencyInjection;
using StreamWave.Extensions;

namespace StreamWave.Tests;

public class RegistrationTest
{
    [Fact]
    public async Task AddAggregate_Test()
    {
        var services = new ServiceCollection();

        services.AddAggregate<TestState, Guid>(() => new TestState {  Id = Guid.NewGuid() })
            .WithApplier<CreatedEvent>((state, e) => {
                state.Id = e.Id;
                return Task.FromResult(state);
            });

        var provider = services.BuildServiceProvider();

       
        var manager = provider.GetRequiredService<IAggregateManager<TestState, Guid>>();

        var aggregate = await manager.LoadAsync(Guid.NewGuid());
        var aggregate1 = await manager.LoadAsync(Guid.NewGuid());

        aggregate.Should().NotBeNull();
        aggregate.Stream.Id.Should().Be(aggregate.State.Id);

        aggregate1.State.Id.Should().NotBe(aggregate.State.Id);
        aggregate1.Stream.Id.Should().NotBe(aggregate.Stream.Id);
    }
}

public record CreatedEvent(Guid Id);
