using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StreamWave.Extensions;
using System.Text.Json;

namespace StreamWave.EntityFramework.Tests;

public class StorageRegistration
{
    [Fact]
    public async Task register_storage_full_specs()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestContext>(x => x.UseInMemoryDatabase("test"), contextLifetime: ServiceLifetime.Transient);
        services.AddSingleton(JsonSerializerOptions.Default);
        services.AddScoped<IEventSerializer, DefaultSerializer>();

        services.AddAggregate<TestState, Guid>(() => new())
            .WithStreamLoader((s) =>
                  (id) =>
                  {
                      var context = s.GetRequiredService<TestContext>();
                      var serializer = s.GetRequiredService<IEventSerializer>();
                      return new Store<TestState, Guid>(context, serializer).LoadStreamAsync(id);
                  }
            )
            .WithSaver((s) =>
                (a) => {
                    var context = s.GetRequiredService<TestContext>();
                    var serializer = s.GetRequiredService<IEventSerializer>();
                    return new Store<TestState, Guid>(context, serializer).SaveAsync(a);
                })
            .WithApplier<TestEvent>((s, e) =>
            {
                s.Test = e.Field;
                return Task.FromResult(s);
            }); 

        var provider = services.BuildServiceProvider();

        var manager = provider.GetRequiredService<IAggregateManager<TestState, Guid>>();

        var aggregate = await manager.Create();

        await aggregate.ApplyAsync(new TestEvent("test"));

        await manager.SaveAsync(aggregate);

        await manager.LoadAsync(aggregate.Stream.Id);

        aggregate.Should().NotBeNull();
    }

    [Fact]
    public async Task register_storage_with_extension()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestContext>(x => x.UseInMemoryDatabase("test"), contextLifetime: ServiceLifetime.Transient);

        services.AddSingleton(JsonSerializerOptions.Default);
        services.AddScoped<IEventSerializer, DefaultSerializer>();

        services.AddAggregate<TestState, Guid>(() => new TestState() {  Id = Guid.NewGuid() })
            .WithEntityFramework<TestContext, TestState, Guid>()
            .WithApplier<TestEvent>((s, e) =>
            {
                s.Test = e.Field;
                return Task.FromResult(s);
            });

        var provider = services.BuildServiceProvider();
        var manager = provider.GetRequiredService<IAggregateManager<TestState, Guid>>();

        var aggregate = await manager.Create();
        var aggregate1 = await manager.Create();

        await aggregate.ApplyAsync(new TestEvent("Aggregate 0"));
        await aggregate1.ApplyAsync(new TestEvent("Aggregate 1"));

        await manager.SaveAsync(aggregate1);
        await manager.SaveAsync(aggregate);

        aggregate = await manager.LoadAsync(aggregate1.Stream.Id);

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

public record TestEvent(string Field);

public class TestState : IAggregateState<Guid>
{ 
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Test { get; set; } = string.Empty;
}
