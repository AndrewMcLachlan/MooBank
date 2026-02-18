using Asm.Domain;
using Asm.MooBank.Domain.Entities.ReferenceData;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        // Required for computed columns
        builder.ToTable(tb => tb.UseSqlOutputClause(false));
    }
}
