namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class VirtualAccount : IEntityTypeConfiguration<Domain.Entities.Account.VirtualInstrument>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Account.VirtualInstrument> entity)
    {
        entity.ToTable("VirtualInstrument", tb => tb.Property(e => e.Id).HasColumnName("InstrumentId"));

        entity.Property(e => e.Id).HasColumnName("InstrumentId");
    }
}
