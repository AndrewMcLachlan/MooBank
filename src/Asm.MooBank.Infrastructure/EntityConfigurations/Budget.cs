using Asm.MooBank.Domain.Entities.Budget;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> entity)
    {
        entity.HasKey(x => x.Id);

        entity.HasOne(x => x.Account).WithMany().HasForeignKey(x => x.AccountId);

        entity.HasIndex(x => new { x.AccountId, x.Year }).IsUnique();

        entity.HasMany(x => x.Lines).WithOne(x => x.Budget);
    }
}
