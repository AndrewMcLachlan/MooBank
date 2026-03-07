using Asm.MooBank.Domain.Entities.Forecast;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class ForecastPlanConfiguration : IEntityTypeConfiguration<ForecastPlan>
{
    public void Configure(EntityTypeBuilder<ForecastPlan> entity)
    {
        entity.Property(x => x.AccountScopeMode).HasColumnType("tinyint");
        entity.Property(x => x.StartingBalanceMode).HasColumnType("tinyint");

        entity.Navigation(x => x.Accounts).AutoInclude();
        entity.Navigation(x => x.PlannedItems).AutoInclude();
    }
}
