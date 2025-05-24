using Asm.MooBank.Domain.Entities.ReferenceData;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;
internal class CpiChangeConfiguration : IEntityTypeConfiguration<CpiChange>
{
    public void Configure(EntityTypeBuilder<CpiChange> builder)
    {
        builder.UseTptMappingStrategy();

        builder.ToTable(tb => tb.UseSqlOutputClause(false));

        builder.OwnsOne(e => e.Quarter,
            b =>
            {
                b.Property(e => e.Year).HasColumnName("Year");
                b.Property(e => e.QuarterNumber).HasColumnName("Quarter");
                b.ToTable("CpiChange");
            });


    }
}
