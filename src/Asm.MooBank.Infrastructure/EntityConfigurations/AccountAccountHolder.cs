namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class AccountAccountHolderConfiguration : IEntityTypeConfiguration<Domain.Entities.Account.AccountAccountHolder>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Account.AccountAccountHolder> entity)
    {
        entity.HasKey(entity => new { entity.AccountId, entity.AccountHolderId });

        entity.HasOne(entity => entity.AccountGroup)
              .WithMany()
              .HasForeignKey(entity => entity.AccountGroupId);


        entity.HasOne(entity => entity.AccountHolder)
              .WithMany()
              .HasForeignKey(entity => entity.AccountHolderId);
    }
}