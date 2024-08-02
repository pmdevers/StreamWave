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

        services.AddAggregate<TestState, Guid>((id) => new() { Id = id })
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
                return s;
            }); 

        var provider = services.BuildServiceProvider();

        var manager = provider.GetRequiredService<IAggregateManager<TestState, Guid>>();

        var aggregate = manager.Create(Guid.NewGuid());

        await aggregate.ApplyAsync(new TestEvent("test"));

        await manager.SaveAsync(aggregate);

        await manager.LoadAsync(aggregate.Id);

        aggregate.Should().NotBeNull();
    }

    [Fact]
    public async Task register_storage_with_extension()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestContext>(x => x.UseInMemoryDatabase("test"), contextLifetime: ServiceLifetime.Transient);

        services.AddSingleton(JsonSerializerOptions.Default);
        services.AddScoped<IEventSerializer, DefaultSerializer>();

        services.AddAggregate<TestState, Guid>((id) => new TestState() {  Id = id })
            .WithEntityFramework<TestContext, TestState, Guid>()
            .WithApplier<TestEvent>((s, e) =>
            {
                s.Test = e.Field;
                return s;
            });

        var provider = services.BuildServiceProvider();
        var manager = provider.GetRequiredService<IAggregateManager<TestState, Guid>>();

        var aggregate = manager.Create(Guid.NewGuid());
        var aggregate1 = manager.Create(Guid.NewGuid());

        await aggregate.ApplyAsync(new TestEvent("Aggregate 0"));
        await aggregate1.ApplyAsync(new TestEvent("Aggregate 1"));

        await manager.SaveAsync(aggregate1);
        await manager.SaveAsync(aggregate);

        aggregate = await manager.LoadAsync(aggregate1.Id);

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

public class TestState
{ 
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Test { get; set; } = string.Empty;
}
