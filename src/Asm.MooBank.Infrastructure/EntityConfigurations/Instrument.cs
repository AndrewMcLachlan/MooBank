using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class InstrumentConfiguration : IEntityTypeConfiguration<Instrument>
{
    public void Configure(EntityTypeBuilder<Instrument> entity)
    {
        // Required for computed columns
        entity.ToTable(t => t.HasTrigger("ComputedColumns"));

        entity.ToTable("Instrument", tb => tb.Property(e => e.Id).HasColumnName("Id"));


        entity.Property(e => e.Id).ValueGeneratedOnAdd().HasDefaultValueSql("(newid())");

        entity.Property(r => r.Controller)
            .HasConversion(e => (int)e, e => (Models.Controller)e);

        entity.Property(e => e.LastUpdated)
            .HasColumnType("datetimeoffset(0)")
            .HasDefaultValueSql("(sysutcdatetime())");

        entity.HasMany(p => p.Owners)
               .WithOne(e => e.Instrument)
               .HasPrincipalKey(e => e.Id)
               .HasForeignKey(p => p.InstrumentId);

        entity.HasMany(e => e.VirtualInstruments)
            .WithOne()
            .HasForeignKey(e => e.ParentInstrumentId);
    }
}
