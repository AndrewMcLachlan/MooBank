using Asm.MooBank.Domain.Entities.User;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class AccountHolderCardConfiguration : IEntityTypeConfiguration<UserCard>
{
    public void Configure(EntityTypeBuilder<UserCard> entity)
    {
        entity.HasKey(e => new { e.UserId, e.Last4Digits });
    }
}
