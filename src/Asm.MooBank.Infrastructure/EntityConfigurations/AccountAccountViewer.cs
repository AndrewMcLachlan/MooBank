namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class AccountAccountViewerConfiguration : IEntityTypeConfiguration<Domain.Entities.Account.AccountAccountViewer>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Account.AccountAccountViewer> entity)
    {
        entity.HasKey(entity => new { entity.AccountId, entity.AccountHolderId });

        entity.HasOne(entity => entity.Account)
            .WithMany(account => account.AccountViewers)
                .HasForeignKey(entity => entity.AccountId);

        entity.HasOne(entity => entity.AccountGroup)
              .WithMany()
              .HasForeignKey(entity => entity.AccountGroupId);

        entity.HasOne(entity => entity.AccountHolder)
              .WithMany()
              .HasForeignKey(entity => entity.AccountHolderId);
    }
}
