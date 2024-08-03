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
                Guid.NewGuid(),
                new(),
                new List<EventData>().ToAsyncEnumerable(),
                applier: (s, _) => s,
                validator: _ => []
             );

            aggregate.Should().NotBeNull();
            aggregate.State.Should().NotBeNull();
            aggregate.IsValid.Should().BeTrue();

            aggregate.Messages.Should().NotBeNull().And.HaveCount(0);
        }
    }

    public class Apply
    {
        [Fact]
        public async Task should_call_applier()
        {
            var applier = Substitute.For<ApplyEventDelegate<TestState>>();

            applier(Arg.Any<TestState>(), Arg.Any<object>())
                .ReturnsForAnyArgs(new TestState { Property = "test" });


            var aggregate = new Aggregate<TestState, Guid>(
                Guid.NewGuid(),
                new(),
                new List<EventData>().ToAsyncEnumerable(),
                applier: applier,
                validator: AggregateBuilderDefaults.DefaultValidator<TestState>([])
             );

            await aggregate.ApplyAsync(new EmptyEvent());

            aggregate.State.Property.Should().Be("test");
            aggregate.IsValid.Should().BeTrue();
            aggregate.Version.Should().Be(0);
            aggregate.ExpectedVersion.Should().Be(1);
        }
    }

    public class State
    {
        [Fact]
        public async Task should_call_applier()
        {
            var applier = Substitute.For<ApplyEventDelegate<TestState>>();

            applier(Arg.Any<TestState>(), Arg.Any<object>())
                .Returns(new TestState());


            var aggregate = new Aggregate<TestState, Guid>(
               Guid.NewGuid(),
               new(),
               new List<EventData>().ToAsyncEnumerable(),
               applier: applier,
               validator: AggregateBuilderDefaults.DefaultValidator<TestState>([])
            );

            aggregate.Version.Should().Be(0);
            aggregate.ExpectedVersion.Should().Be(0);

            await aggregate.ApplyAsync(new EmptyEvent());

            aggregate.IsValid.Should().BeTrue();
            aggregate.Version.Should().Be(0);
            aggregate.ExpectedVersion.Should().Be(1);

#pragma warning disable S2970 // Assertions should be complete
            applier.Received(1);
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
                Guid.NewGuid(),
                new(),
                new List<EventData>().ToAsyncEnumerable(),
                applier: (s, _) => s,
                validator: validator
            );

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
                Guid.NewGuid(),
                new(),
                new List<EventData>().ToAsyncEnumerable(),
                applier: (s, _) => s,
                validator: validator
            );

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

            var aggregate = new Aggregate<TestState, Guid>(
               Guid.NewGuid(),
               new(),
               new List<EventData>() { EventData.Create(new EmptyEvent()) }.ToAsyncEnumerable(),
               applier: (s, _) => s,
               validator: AggregateBuilderDefaults.DefaultValidator<TestState>([])
            );

            aggregate.Version.Should().Be(1);
            aggregate.ExpectedVersion.Should().Be(1);

            await aggregate.ApplyAsync(new EmptyEvent());

            aggregate.IsValid.Should().BeTrue();
            aggregate.Version.Should().Be(1);
            aggregate.ExpectedVersion.Should().Be(2);

        }
    }
}
