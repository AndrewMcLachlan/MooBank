namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class LogicalAccount : IEntityTypeConfiguration<Domain.Entities.Account.LogicalAccount>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Account.LogicalAccount> entity)
    {
        entity.Property(e => e.Id).HasColumnName("InstrumentId");
    }
}
