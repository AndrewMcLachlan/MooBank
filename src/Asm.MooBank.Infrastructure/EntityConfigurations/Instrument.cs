using Asm.MooBank.Domain.Entities.Account;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class InstrumentConfiguration : IEntityTypeConfiguration<Domain.Entities.Account.Instrument>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Account.Instrument> entity)
    {
        // Required for computed columns
        entity.ToTable(t => t.HasTrigger("ComputedColumns"));

        entity.HasKey(e => e.Id);

        entity.ToTable("Instrument", tb => tb.Property(e => e.Id).HasColumnName("Id"));

        entity.HasIndex(e => e.Name).IsUnique();

        entity.Property(e => e.Id).ValueGeneratedOnAdd();
        entity.Property(e => e.Id).HasDefaultValueSql("(newid())");

        entity.Property(e => e.Balance).HasColumnType("decimal(12, 4)");

        entity.Property(e => e.Description).HasMaxLength(255);

        entity.Property(e => e.LastUpdated)
            .HasColumnType("datetimeoffset(0)")
            .HasDefaultValueSql("(sysutcdatetime())");

        entity.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(50);

        entity.HasMany(p => p.Owners)
               .WithOne(e => e.Instrument)
               .HasPrincipalKey(e => e.Id)
               .HasForeignKey(p => p.InstrumentId);

        entity.HasMany(e => e.VirtualInstruments)
            .WithOne()
            .HasForeignKey(e => e.ParentInstrumentId);
    }
}
