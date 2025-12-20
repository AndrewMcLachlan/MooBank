CREATE TABLE [dbo].[PlannedItemSchedule]
(
    [PlannedItemId] UNIQUEIDENTIFIER NOT NULL,
    [Frequency] TINYINT NOT NULL,
    [AnchorDate] DATE NOT NULL,
    [Interval] INT NOT NULL CONSTRAINT DF_PlannedItemSchedule_Interval DEFAULT 1,
    [DayOfMonth] INT NULL,
    [EndDate] DATE NULL,
    CONSTRAINT [PK_PlannedItemSchedule] PRIMARY KEY CLUSTERED ([PlannedItemId]),
    CONSTRAINT [FK_PlannedItemSchedule_PlannedItem] FOREIGN KEY ([PlannedItemId]) REFERENCES [ForecastPlannedItem]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_PlannedItemSchedule_Frequency] FOREIGN KEY ([Frequency]) REFERENCES [ScheduleFrequency]([Id])
)
GO
