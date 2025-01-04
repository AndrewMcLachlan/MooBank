using Asm.MooBank.Domain.Entities.Utility;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class UtilityAccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Account", "utilities");
        builder.Property(e => e.Id).HasColumnName("InstrumentId");
    }
}
