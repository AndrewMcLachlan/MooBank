CREATE TABLE [dbo].[PlannedItemFlexibleWindow]
(
    [PlannedItemId] UNIQUEIDENTIFIER NOT NULL,
    [StartDate] DATE NOT NULL,
    [EndDate] DATE NOT NULL,
    [AllocationMode] TINYINT NOT NULL,
    CONSTRAINT [PK_PlannedItemFlexibleWindow] PRIMARY KEY CLUSTERED ([PlannedItemId]),
    CONSTRAINT [FK_PlannedItemFlexibleWindow_PlannedItem] FOREIGN KEY ([PlannedItemId]) REFERENCES [ForecastPlannedItem]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PlannedItemFlexibleWindow_AllocationMode] FOREIGN KEY ([AllocationMode]) REFERENCES [AllocationMode]([Id])
)
GO
