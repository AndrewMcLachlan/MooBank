CREATE TABLE [dbo].[PlannedItemFixedDate]
(
    [PlannedItemId] UNIQUEIDENTIFIER NOT NULL,
    [FixedDate] DATE NOT NULL,
    CONSTRAINT [PK_PlannedItemFixedDate] PRIMARY KEY CLUSTERED ([PlannedItemId]),
    CONSTRAINT [FK_PlannedItemFixedDate_PlannedItem] FOREIGN KEY ([PlannedItemId]) REFERENCES [ForecastPlannedItem]([Id]) ON DELETE CASCADE
)
GO
