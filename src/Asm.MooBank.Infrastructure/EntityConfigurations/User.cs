using Asm.MooBank.Domain.Entities.User;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

public class AccountHolderConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.HasMany(p => p.InstrumentOwners)
            .WithOne(e => e.User)
            .HasPrincipalKey(e => e.Id)
            .HasForeignKey(p => p.UserId);
    }
}
