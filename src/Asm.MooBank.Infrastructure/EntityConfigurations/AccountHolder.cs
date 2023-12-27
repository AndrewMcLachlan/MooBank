using Asm.MooBank.Domain.Entities.AccountHolder;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class AccountHolderConfiguration : IEntityTypeConfiguration<AccountHolder>
{
    public void Configure(EntityTypeBuilder<AccountHolder> entity)
    {
        entity.HasIndex(e => e.EmailAddress)
            .HasDatabaseName("IX_AccountHolder_Email")
            .IsUnique();

        entity.HasKey(e => e.Id);
        entity.Property(e => e.Id).HasColumnName("AccountHolderId");

        entity.Property(e => e.EmailAddress)
            .IsRequired()
            .HasMaxLength(255);

        entity.Property(e => e.FirstName)
            .HasMaxLength(255);

        entity.Property(e => e.LastName)
            .HasMaxLength(255);

        entity.HasMany(p => p.AccountAccountHolders)
            .WithOne(e => e.AccountHolder)
            .HasPrincipalKey(e => e.Id)
            .HasForeignKey(p => p.AccountHolderId);

        entity.HasMany(e => e.Cards).WithOne(e => e.AccountHolder).HasForeignKey(e => e.AccountHolderId);

        entity.HasOne(e => e.Family).WithMany(e => e.AccountHolders).HasForeignKey(e => e.FamilyId);
    }
}
