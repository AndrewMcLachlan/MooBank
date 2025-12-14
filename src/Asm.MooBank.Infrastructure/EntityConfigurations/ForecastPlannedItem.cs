using Asm.MooBank.Domain.Entities.Forecast;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class ForecastPlannedItemConfiguration : IEntityTypeConfiguration<ForecastPlannedItem>
{
    public void Configure(EntityTypeBuilder<ForecastPlannedItem> entity)
    {
        entity.Property(x => x.ItemType).HasColumnType("tinyint");
        entity.Property(x => x.DateMode).HasColumnType("tinyint");
        entity.Property(x => x.ScheduleFrequency).HasColumnType("tinyint");
        entity.Property(x => x.AllocationMode).HasColumnType("tinyint");

        entity.Navigation(x => x.Tag).AutoInclude();
    }
}
