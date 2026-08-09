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
    [Notes] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_ForecastPlannedItem] PRIMARY KEY CLUSTERED ([Id]),
    -- Redundant on its own -- Id is already unique -- but it gives ForecastPlannedItemTransaction
    -- something to point a composite foreign key at, so a link row cannot name one plan while its
    -- item belongs to another.
    CONSTRAINT [UQ_ForecastPlannedItem_Id_ForecastPlanId] UNIQUE ([Id], [ForecastPlanId]),
    CONSTRAINT [FK_ForecastPlannedItem_ForecastPlan] FOREIGN KEY ([ForecastPlanId]) REFERENCES [ForecastPlan]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ForecastPlannedItem_ItemType] FOREIGN KEY ([ItemType]) REFERENCES [PlannedItemType]([Id]),
    CONSTRAINT [FK_ForecastPlannedItem_DateMode] FOREIGN KEY ([DateMode]) REFERENCES [PlannedItemDateMode]([Id]),
    CONSTRAINT [FK_ForecastPlannedItem_Tag] FOREIGN KEY ([TagId]) REFERENCES [Tag]([Id]),
    CONSTRAINT [FK_ForecastPlannedItem_VirtualInstrument] FOREIGN KEY ([VirtualInstrumentId]) REFERENCES [VirtualInstrument]([InstrumentId])
)
GO

CREATE INDEX [IX_ForecastPlannedItem_ForecastPlanId] ON [dbo].[ForecastPlannedItem] ([ForecastPlanId])
GO
