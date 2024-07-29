using System.Security.Cryptography.X509Certificates;

namespace StreamWave.Tests;

public class TestState
{
    public string Property { get; set; } = string.Empty;
}

public class AggregateTest
{
    public class Constructor
    {
        [Fact]
        public void Should_have_default_values()
        {
            var aggregate = new Aggregate<TestState>(
                creator: () => new(),
                applier: (s, _) => s,
                validator: _ => [],
                loader: id => {
                    return Task.FromResult<IEventStream?>(EventStream.Create());
                },
                saver: s => Task.FromResult(s.Stream.Commit()));

            aggregate.Should().NotBeNull();
            aggregate.Stream.Should().NotBeNull();
            aggregate.State.Should().NotBeNull();
            aggregate.IsValid.Should().BeTrue();

            aggregate.Messages.Should().NotBeNull().And.HaveCount(0);
        }
    }

    public class LoadAsync
    {
        [Fact]
        public async Task should_call_loader_with_same_id()
        { 
            var guid = StreamId.Guid();
            
            var aggregate = new Aggregate<TestState>(
                creator: () => new(), 
                applier: (s, _) => s, 
                validator: _ => [], 
                loader: id => { 
                    id.Should().Be(guid);
                    return Task.FromResult<IEventStream?>(EventStream.Create(id));
                }, 
                saver: s => Task.FromResult(s.Stream.Commit()));

            await aggregate.LoadAsync(guid);

            aggregate.Should().NotBeNull();
            aggregate.Stream.Should().NotBeNull();
            aggregate.State.Should().NotBeNull();
            aggregate.IsValid.Should().BeTrue();
            
            aggregate.Messages.Should().NotBeNull().And.HaveCount(0);
            aggregate.Stream.Id.Should().Be(guid);
        }
    }

    public class Apply
    {
        [Fact]
        public void should_call_applier()
        {
            Assert.False(true);
        }
    }

    public class SaveAsync
    {
        [Fact]
        public void should_call_saver()
        {
            Assert.False(true);
        }
    }

    public class State
    {
        [Fact]
        public void should_call_applier()
        {
            Assert.False(true);
        }
    }

    public class Messages
    {
        [Fact]
        public void should_call_validator()
        {
            Assert.False(true);
        }

        [Fact]
        public void should_return_validation_messages()
        {
            Assert.False(true);
        }
    }

    public class Stream
    {
        [Fact]
        public void should_return_loaded_stream()
        {
            Assert.False(true);
        }
    }
}
