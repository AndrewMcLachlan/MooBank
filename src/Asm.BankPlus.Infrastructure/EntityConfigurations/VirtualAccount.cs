namespace Asm.BankPlus.Infrastructure.EntityConfigurations;

public class VirtualAccount : IEntityTypeConfiguration<Data.Entities.VirtualAccount>
{
    public void Configure(EntityTypeBuilder<Data.Entities.VirtualAccount> entity)
    {
        entity.Property(e => e.VirtualAccountId).HasDefaultValueSql("(newid())");

        entity.Property(e => e.Balance).HasColumnType("decimal(10, 2)");

        entity.Property(e => e.Description)
              .HasMaxLength(255)
              .IsUnicode(false);

        entity.Property(e => e.Name)
              .IsRequired()
              .HasMaxLength(50)
              .IsUnicode(false);

        entity.HasOne(e => e.Account).WithMany(e => e.VirtualAccounts).HasForeignKey(e => e.AccountId);


    }
}
