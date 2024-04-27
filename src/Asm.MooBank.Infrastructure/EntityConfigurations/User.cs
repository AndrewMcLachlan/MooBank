using Asm.MooBank.Domain.Entities.User;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class AccountHolderConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasIndex(e => e.EmailAddress)
            .HasDatabaseName("IX_User_Email")
            .IsUnique();

        entity.HasKey(e => e.Id);

        entity.Property(e => e.EmailAddress)
            .IsRequired()
            .HasMaxLength(255);

        entity.Property(e => e.FirstName)
            .HasMaxLength(255);

        entity.Property(e => e.LastName)
            .HasMaxLength(255);

        entity.HasMany(p => p.InstrumentOwners)
            .WithOne(e => e.User)
            .HasPrincipalKey(e => e.Id)
            .HasForeignKey(p => p.UserId);

        entity.HasMany(e => e.Cards).WithOne(e => e.AccountHolder).HasForeignKey(e => e.AccountHolderId);

        entity.HasOne(e => e.Family).WithMany(e => e.AccountHolders).HasForeignKey(e => e.FamilyId);
    }
}
