-- Payments the plan's author has said belong to a planned item.
--
-- Tags cannot identify an item on their own: a tag is a category and a planned item is a specific
-- project, so one "Home Improvements" tag covers the solar panels, the fence and the renovation.
-- The tag narrows the candidates offered for linking; this table records the author's answer.
CREATE TABLE [dbo].[ForecastPlannedItemTransaction]
(
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_ForecastPlannedItemTransaction_Id DEFAULT NEWID(),
    [PlannedItemId] UNIQUEIDENTIFIER NOT NULL,
    -- Denormalised from the item so a payment can be claimed by only one item within a plan.
    [ForecastPlanId] UNIQUEIDENTIFIER NOT NULL,
    [TransactionId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_ForecastPlannedItemTransaction] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_ForecastPlannedItemTransaction_PlannedItem] FOREIGN KEY ([PlannedItemId]) REFERENCES [ForecastPlannedItem]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ForecastPlannedItemTransaction_ForecastPlan] FOREIGN KEY ([ForecastPlanId]) REFERENCES [ForecastPlan]([Id]),
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
