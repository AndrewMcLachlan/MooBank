using Asm.MooBank.Domain.Entities.Budget;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class BudgetLineConfiguration : IEntityTypeConfiguration<BudgetLine>
{
    public void Configure(EntityTypeBuilder<BudgetLine> entity)
    {
        entity.HasKey(x => x.Id);

        //entity.HasOne(x => x.Budget).WithMany().HasForeignKey(x => x.BudgetId);

        entity.HasOne(x => x.Tag).WithMany().HasForeignKey(x => x.TagId);

        entity.Property(x => x.Amount).HasPrecision(12, 4);

        entity.Navigation(x => x.Tag).AutoInclude();
    }
}
