using Asm.MooBank.Domain.Entities.AccountHolder;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class AccountHolderCardConfiguration : IEntityTypeConfiguration<AccountHolderCard>
{
    public void Configure(EntityTypeBuilder<AccountHolderCard> entity)
    {
        entity.HasKey(e => new { e.AccountHolderId, e.Last4Digits });
    }
}
