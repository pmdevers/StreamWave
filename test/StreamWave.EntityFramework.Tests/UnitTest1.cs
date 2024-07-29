using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace StreamWave.EntityFramework.Tests;

public class StorageRegistration
{
    [Fact]
    public async Task register_storage_full_specs()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestContext>(x => x.UseInMemoryDatabase("test"), contextLifetime: ServiceLifetime.Transient);

        services.AddAggregate<TestState, Guid>(() => new())
            .WithLoader((s) =>
                  (id) =>
                  {
                      var context = s.GetRequiredService<TestContext>();
                      return AggregateStore<TestState, Guid>.LoadAsync(context, id);
                  }
            )
            .WithSaver((s) =>
                (a) => { 
                    var context = s.GetRequiredService<TestContext>();
                    return AggregateStore<TestState, Guid>.SaveAsync(context, a);
                })
            .WithApplier<TestEvent>((s, e) =>
            {
                s.Test = e.Field;
                return s;
            }); 

        var provider = services.BuildServiceProvider();

        var aggregate = provider.GetRequiredService<IAggregate<TestState, Guid>>();

        aggregate.Apply(new TestEvent("test"));

        await aggregate.SaveAsync();

        await aggregate.LoadAsync(aggregate.Stream.Id);

        aggregate.Should().NotBeNull();
    }

    [Fact]
    public async Task register_storage_with_extension()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestContext>(x => x.UseInMemoryDatabase("test"), contextLifetime: ServiceLifetime.Transient);

        services.AddAggregate<TestState, Guid>(() => new TestState() {  Id = Guid.NewGuid() })
            .WithEntityFramework<TestContext, TestState, Guid>()
            .WithApplier<TestEvent>((s, e) =>
            {
                s.Test = e.Field;
                return s;
            });

        var provider = services.BuildServiceProvider();

        var aggregate = provider.GetRequiredService<IAggregate<TestState, Guid>>();
        var aggregate1 = provider.GetRequiredService<IAggregate<TestState, Guid>>();

        aggregate.Apply(new TestEvent("Aggregate 0"));
        aggregate1.Apply(new TestEvent("Aggregate 1"));

        await aggregate.SaveAsync();
        await aggregate1.SaveAsync();

        await aggregate.LoadAsync(aggregate1.Stream.Id);

        aggregate.Should().NotBeNull();
        aggregate.State.Id.Should().Be(aggregate1.State.Id);
    }
}

public class TestContext(DbContextOptions<TestContext> options) : DbContext(options)
{
    public DbSet<TestState> Entities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestState>()
            .HasKey(x => x.Id);

        modelBuilder.ApplyConfiguration(new AggregateTypeConfiguration<TestState, Guid>());
    }
}

public record TestEvent(string Field) : Event;

public class TestState : IAggregateState<Guid>
{ 
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Test { get; set; } = string.Empty;
}
