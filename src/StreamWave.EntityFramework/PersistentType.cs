using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace StreamWave.EntityFramework;
public sealed class AggregateTypeConfiguration<TState, TId>
    : IEntityTypeConfiguration<PersistendEvent<TId>>
{
    public void Configure(EntityTypeBuilder<PersistendEvent<TId>> builder)
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
