CREATE TABLE [dbo].[ForecastPlannedItem]
(
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_ForecastPlannedItem_Id DEFAULT NEWID(),
    [ForecastPlanId] UNIQUEIDENTIFIER NOT NULL,
    [ItemType] TINYINT NOT NULL,
    [Name] NVARCHAR(200) NOT NULL,
    [Amount] DECIMAL(18,2) NOT NULL,
    [TagId] INT NULL,
    [VirtualInstrumentId] UNIQUEIDENTIFIER NULL,
    [IsIncluded] BIT NOT NULL CONSTRAINT DF_ForecastPlannedItem_IsIncluded DEFAULT 1,
    [DateMode] TINYINT NOT NULL,
    -- Fixed date fields
    [FixedDate] DATE NULL,
    -- Schedule fields
    [ScheduleFrequency] TINYINT NULL,
    [ScheduleAnchorDate] DATE NULL,
    [ScheduleInterval] INT NULL,
    [ScheduleDayOfMonth] INT NULL,
    [ScheduleEndDate] DATE NULL,
    -- Flexible window fields (V1)
    [WindowStartDate] DATE NULL,
    [WindowEndDate] DATE NULL,
    [AllocationMode] TINYINT NULL,
    [Notes] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_ForecastPlannedItem] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_ForecastPlannedItem_ForecastPlan] FOREIGN KEY ([ForecastPlanId]) REFERENCES [ForecastPlan]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ForecastPlannedItem_Tag] FOREIGN KEY ([TagId]) REFERENCES [Tag]([Id]),
    CONSTRAINT [FK_ForecastPlannedItem_VirtualInstrument] FOREIGN KEY ([VirtualInstrumentId]) REFERENCES [VirtualInstrument]([InstrumentId])
)
GO

CREATE INDEX [IX_ForecastPlannedItem_ForecastPlanId] ON [dbo].[ForecastPlannedItem] ([ForecastPlanId])
GO
