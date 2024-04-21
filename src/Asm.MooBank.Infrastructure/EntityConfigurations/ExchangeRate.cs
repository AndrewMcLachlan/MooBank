using Asm.Domain;
using Asm.MooBank.Domain.Entities.ReferenceData;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;
internal class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        // Required for computed columns
        builder.ToTable(t => t.HasTrigger("ComputedColumns"));

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Rate).HasColumnType("decimal(12, 4)");

        builder.Property(e => e.ReverseRate)
            .ValueGeneratedOnAddOrUpdate();
    }
}
