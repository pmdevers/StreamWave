using System.ComponentModel.DataAnnotations;

namespace StreamWave.Tests;

public class AggregateTest
{
    public class Constructor
    {
        [Fact]
        public void Should_have_default_values()
        {
            var aggregate = new Aggregate<TestState, Guid>(
                creator: () => new(),
                applier: (s, _) => Task.FromResult(s),
                validator: _ => [],
                loader: id => {
                    return Task.FromResult<IEventStream<Guid>?>(EventStream.Create(Guid.Empty));
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
            var guid = Guid.NewGuid();
            var loader = Substitute.For<LoadEventStreamDelegate<Guid>>();

            loader(Arg.Is(guid))
                .Returns(Task.FromResult<IEventStream<Guid>?>(EventStream.Create(guid)));
                
            
            var aggregate = new Aggregate<TestState, Guid>(
                creator: () => new(), 
                applier: (s, _) => Task.FromResult(s), 
                validator: _ => [], 
                loader: (id) => loader(id), 
                saver: s => Task.FromResult(s.Stream.Commit()));

            await aggregate.LoadAsync(guid);


            _ = loader.Received(1);

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
        public async Task should_call_applier()
        {
            var applier = Substitute.For<ApplyEventDelegate<TestState>>();
                
            applier(Arg.Any<TestState>(), Arg.Any<Event>())
                .ReturnsForAnyArgs(new TestState { Property = "test" });
            

            var aggregate = new Aggregate<TestState, Guid>(
                creator: () => new(),
                applier: applier,
                validator: AggregateBuilderDefaults.DefaultValidator<TestState>([]),
                loader: AggregateBuilderDefaults.DefaultLoader<Guid>(),
                saver: s => Task.FromResult(s.Stream.Commit()));

            await aggregate.ApplyAsync(new EmptyEvent());

            aggregate.State.Property.Should().Be("test");
            aggregate.IsValid.Should().BeTrue();
            aggregate.Stream.Version.Should().Be(0);
            aggregate.Stream.ExpectedVersion.Should().Be(1);
        }
    }
    
    public class SaveAsync
    {
        [Fact]
        public async Task should_call_saver()
        {
            var aggregate = new Aggregate<TestState, Guid>(
                creator: () => new(),
                applier: (s, _) => Task.FromResult(s),
                validator: AggregateBuilderDefaults.DefaultValidator<TestState>([]),
                loader: AggregateBuilderDefaults.DefaultLoader<Guid>(),
                saver: s => Task.FromResult(s.Stream.Commit()));

            aggregate.Stream.Version.Should().Be(0);
            aggregate.Stream.ExpectedVersion.Should().Be(0);

            await aggregate.ApplyAsync(new EmptyEvent());

            await aggregate.SaveAsync();

            aggregate.IsValid.Should().BeTrue();
            aggregate.Stream.Version.Should().Be(1);
            aggregate.Stream.ExpectedVersion.Should().Be(1);
        }
    }

    public class State
    {
        [Fact]
        public async Task should_call_applier()
        {
            var loader = Substitute.For<ApplyEventDelegate<TestState>>();

            loader(Arg.Any<TestState>(), Arg.Any<Event>())
                .Returns(new TestState());


            var aggregate = new Aggregate<TestState, Guid>(
               creator: () => new(),
               applier: loader,
               validator: AggregateBuilderDefaults.DefaultValidator<TestState>([]),
               loader: AggregateBuilderDefaults.DefaultLoader<Guid>(),
               saver: s => Task.FromResult(s.Stream.Commit()));

            aggregate.Stream.Version.Should().Be(0);
            aggregate.Stream.ExpectedVersion.Should().Be(0);

            await aggregate.ApplyAsync(new EmptyEvent());

            aggregate.IsValid.Should().BeTrue();
            aggregate.Stream.Version.Should().Be(0);
            aggregate.Stream.ExpectedVersion.Should().Be(1);

#pragma warning disable S2970 // Assertions should be complete
            loader.Received(1);
#pragma warning restore S2970 // Assertions should be complete

        }
    }

    public class Messages
    {
        [Fact]
        public async Task should_call_validator()
        {
            var validator = Substitute.For<ValidateStateDelegate<TestState>>();

            validator(Arg.Any<TestState>())
                .ReturnsForAnyArgs([]);

            var aggregate = new Aggregate<TestState, Guid>(
                creator: () => new(),
                applier: (s, _) => Task.FromResult(s),
                validator: validator,
                loader: AggregateBuilderDefaults.DefaultLoader<Guid>(),
                saver: s => Task.FromResult(s.Stream.Commit()));

            await aggregate.ApplyAsync(new EmptyEvent());

            aggregate.IsValid.Should().BeTrue();
            aggregate.Messages.Should().BeEmpty();
#pragma warning disable S2970 // Assertions should be complete
            validator.Received(1);
#pragma warning restore S2970 // Assertions should be complete

        }

        [Fact]
        public async Task should_return_validation_messages()
        {
            var validator = Substitute.For<ValidateStateDelegate<TestState>>();

            validator(Arg.Any<TestState>())
                .ReturnsForAnyArgs([new ValidationMessage("Pffff")]);

            var aggregate = new Aggregate<TestState, Guid>(
                creator: () => new(),
                applier: (s, _) => Task.FromResult(s),
                validator: validator,
                loader: AggregateBuilderDefaults.DefaultLoader<Guid>(),
                saver: s => Task.FromResult(s.Stream.Commit()));

            await aggregate.ApplyAsync(new EmptyEvent());

            aggregate.IsValid.Should().BeFalse();
            aggregate.Messages.Should().HaveCount(1);
#pragma warning disable S2970 // Assertions should be complete
            validator.Received(1);
        }
    }

    public class Stream
    {
        [Fact]
        public async Task should_return_loaded_stream()
        {
            var stream = EventStream.Create(Guid.NewGuid(), [
                new EmptyEvent()
                ]);
            var loader = Substitute.For<LoadEventStreamDelegate<Guid>>();

            loader(Arg.Any<Guid>()).Returns(stream);


            var aggregate = new Aggregate<TestState, Guid>(
               creator: () => new(),
               applier: (s, _) => Task.FromResult(s),
               validator: AggregateBuilderDefaults.DefaultValidator<TestState>([]),
               loader: loader,
               saver: s => Task.FromResult(s.Stream.Commit()));

            aggregate.Stream.Version.Should().Be(0);
            aggregate.Stream.ExpectedVersion.Should().Be(0);

            await aggregate.ApplyAsync(new EmptyEvent());

            await aggregate.SaveAsync();

            aggregate.IsValid.Should().BeTrue();
            aggregate.Stream.Version.Should().Be(1);
            aggregate.Stream.ExpectedVersion.Should().Be(1);

            loader.ReceivedCalls();
        }
    }
}
