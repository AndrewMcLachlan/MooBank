namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class InstitutionAccount : IEntityTypeConfiguration<Domain.Entities.Account.InstitutionAccount>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Account.InstitutionAccount> entity)
    {
        entity.Property(e => e.LastUpdated)
            .HasColumnType("datetimeoffset(0)")
            .HasDefaultValueSql("(sysutcdatetime())");

        entity.HasOne(e => e.ImportAccount)
              .WithOne()
            .HasForeignKey<Domain.Entities.Account.ImportAccount>(e => e.AccountId);

        entity.HasOne(e => e.Institution).WithMany().HasForeignKey(e => e.InstitutionId);

    }
}
