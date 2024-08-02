using Microsoft.EntityFrameworkCore;
using SampleApp.Domain;
using StreamWave.EntityFramework;

namespace SampleApp.Infrastructure;

public class SampleDBContext(DbContextOptions<SampleDBContext> options) : DbContext(options)
{
    public DbSet<SampleState> Entities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SampleState>()
            .HasKey(x => x.Id);

        modelBuilder.ApplyConfiguration(new AggregateTypeConfiguration<SampleState, Guid>());
    }
}
