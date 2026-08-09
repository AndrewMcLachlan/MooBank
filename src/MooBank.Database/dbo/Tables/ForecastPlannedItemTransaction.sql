-- Payments the plan's author has said belong to a planned item.
--
-- Tags cannot identify an item on their own: a tag is a category and a planned item is a specific
-- project, so one "Home Improvements" tag covers the solar panels, the fence and the renovation.
-- The tag narrows the candidates offered for linking; this table records the author's answer.
CREATE TABLE [dbo].[ForecastPlannedItemTransaction]
(
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_ForecastPlannedItemTransaction_Id DEFAULT NEWID(),
    [PlannedItemId] UNIQUEIDENTIFIER NOT NULL,
    -- Denormalised from the item so the unique index below can stop two items in one plan claiming
    -- the same payment. The composite foreign key is what keeps it honest: the pair has to match a
    -- real (item, plan) pair, so this cannot drift from the item it belongs to.
    [ForecastPlanId] UNIQUEIDENTIFIER NOT NULL,
    [TransactionId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_ForecastPlannedItemTransaction] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_ForecastPlannedItemTransaction_PlannedItem] FOREIGN KEY ([PlannedItemId], [ForecastPlanId]) REFERENCES [ForecastPlannedItem]([Id], [ForecastPlanId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ForecastPlannedItemTransaction_Transaction] FOREIGN KEY ([TransactionId]) REFERENCES [Transaction]([TransactionId])
)
GO

-- One item per payment per plan. A payment covering two items -- a single school fees invoice for
-- two children -- is a sign the two should be one item.
CREATE UNIQUE INDEX [UX_ForecastPlannedItemTransaction_Plan_Transaction]
    ON [dbo].[ForecastPlannedItemTransaction] ([ForecastPlanId], [TransactionId])
GO

CREATE INDEX [IX_ForecastPlannedItemTransaction_PlannedItemId]
    ON [dbo].[ForecastPlannedItemTransaction] ([PlannedItemId])
GO
