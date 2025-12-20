using Asm.MooBank.Domain.Entities.Forecast;

namespace Asm.MooBank.Infrastructure.EntityConfigurations;

internal class ForecastPlannedItemConfiguration : IEntityTypeConfiguration<ForecastPlannedItem>
{
    public void Configure(EntityTypeBuilder<ForecastPlannedItem> entity)
    {
        entity.Property(x => x.ItemType).HasColumnType("tinyint");
        entity.Property(x => x.DateMode).HasColumnType("tinyint");

        entity.Navigation(x => x.Tag).AutoInclude();

        // Configure 1-to-0..1 relationships with schedule configurations
        entity.HasOne(x => x.FixedDate)
            .WithOne(x => x.PlannedItem)
            .HasForeignKey<PlannedItemFixedDate>(x => x.PlannedItemId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(x => x.Schedule)
            .WithOne(x => x.PlannedItem)
            .HasForeignKey<PlannedItemSchedule>(x => x.PlannedItemId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(x => x.FlexibleWindow)
            .WithOne(x => x.PlannedItem)
            .HasForeignKey<PlannedItemFlexibleWindow>(x => x.PlannedItemId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.Navigation(x => x.FixedDate).AutoInclude();
        entity.Navigation(x => x.Schedule).AutoInclude();
        entity.Navigation(x => x.FlexibleWindow).AutoInclude();
    }
}

internal class PlannedItemFixedDateConfiguration : IEntityTypeConfiguration<PlannedItemFixedDate>
{
    public void Configure(EntityTypeBuilder<PlannedItemFixedDate> entity)
    {
        entity.ToTable("PlannedItemFixedDate");
    }
}

internal class PlannedItemScheduleConfiguration : IEntityTypeConfiguration<PlannedItemSchedule>
{
    public void Configure(EntityTypeBuilder<PlannedItemSchedule> entity)
    {
        entity.ToTable("PlannedItemSchedule");
        entity.Property(x => x.Frequency).HasColumnType("tinyint");
    }
}

internal class PlannedItemFlexibleWindowConfiguration : IEntityTypeConfiguration<PlannedItemFlexibleWindow>
{
    public void Configure(EntityTypeBuilder<PlannedItemFlexibleWindow> entity)
    {
        entity.ToTable("PlannedItemFlexibleWindow");
        entity.Property(x => x.AllocationMode).HasColumnType("tinyint");
    }
}
