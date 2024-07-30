using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace StreamWave.EntityFramework;

/// <summary>
/// Configures the entity type mapping for persisted events in an event-sourced system.
/// This configuration is used with Entity Framework to map the <see cref="PersistedEvent{TId}"/> type to the database.
/// </summary>
/// <typeparam name="TState">The type of the aggregate state associated with the events.</typeparam>
/// <typeparam name="TId">The type of the identifier for the event stream.</typeparam>
public sealed class AggregateTypeConfiguration<TState, TId>
    : IEntityTypeConfiguration<PersistedEvent<TId>>
{
    /// <summary>
    /// Configures the entity type for <see cref="PersistedEvent{TId}"/>, specifying table name, property mappings, and indexes.
    /// </summary>
    /// <param name="builder">The builder used to configure the entity type.</param>
    public void Configure(EntityTypeBuilder<PersistedEvent<TId>> builder)
    {
        var aggregateName = typeof(TState).Name;
        builder.ToTable($"{aggregateName}_Events");

        builder.Property(o => o.StreamId);

        builder.HasIndex(p => new { p.StreamId, p.Version })
            .IsUnique();

        builder.HasIndex(p => p.StreamId)
            .IsUnique(false);
    }
}
