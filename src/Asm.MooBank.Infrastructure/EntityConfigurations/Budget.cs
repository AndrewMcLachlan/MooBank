using Asm.MooBank.Domain.Entities.Budget;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> entity)
    {
        entity.HasKey(x => x.Id);

        entity.HasOne(x => x.Family).WithOne();

        entity.HasIndex(x => new { x.FamilyId, x.Year }).IsUnique();

        entity.HasMany(x => x.Lines).WithOne(x => x.Budget);
    }
}
