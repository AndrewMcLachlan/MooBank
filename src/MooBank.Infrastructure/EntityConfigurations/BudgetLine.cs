using Asm.MooBank.Domain.Entities.Budget;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class BudgetLineConfiguration : IEntityTypeConfiguration<BudgetLine>
{
    public void Configure(EntityTypeBuilder<BudgetLine> entity)
    {
        entity.Navigation(x => x.Tag).AutoInclude();
    }
}
