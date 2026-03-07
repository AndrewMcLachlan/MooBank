using Asm.MooBank.Domain.Entities.Instrument;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class InstrumentConfiguration : IEntityTypeConfiguration<Instrument>
{
    public void Configure(EntityTypeBuilder<Instrument> entity)
    {
        // Required for computed columns
        entity.ToTable(tb => tb.UseSqlOutputClause(false));

        entity.UseTptMappingStrategy();
        entity.ToTable("Instrument", tb => tb.Property(e => e.Id).HasColumnName("Id"));

        entity.HasMany(p => p.Owners)
               .WithOne(e => e.Instrument)
               .HasPrincipalKey(e => e.Id)
               .HasForeignKey(p => p.InstrumentId);

        entity.HasMany(e => e.VirtualInstruments)
            .WithOne()
            .HasForeignKey(e => e.ParentInstrumentId);
    }
}
