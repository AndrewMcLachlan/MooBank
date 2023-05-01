using Asm.MooBank.Domain.Entities.Budget;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class BudgetLineConfiguration : IEntityTypeConfiguration<BudgetLine>
{
    public void Configure(EntityTypeBuilder<BudgetLine> entity)
    {
        entity.HasKey(x => x.Id);

        entity.HasOne(x => x.Account).WithMany().HasForeignKey(x => x.AccountId);

        entity.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId);

        entity.Property(x => x.Amount).HasPrecision(10, 2);
    }
}
