CREATE TABLE [dbo].[ForecastPlanAccount]
(
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_ForecastPlanAccount_Id DEFAULT NEWSEQUENTIALID(),
    [ForecastPlanId] UNIQUEIDENTIFIER NOT NULL,
    [InstrumentId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_ForecastPlanAccount] PRIMARY KEY CLUSTERED ([Id]),
    CONSTRAINT [FK_ForecastPlanAccount_ForecastPlan] FOREIGN KEY ([ForecastPlanId]) REFERENCES [ForecastPlan]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ForecastPlanAccount_Instrument] FOREIGN KEY ([InstrumentId]) REFERENCES [Instrument]([Id]),
    CONSTRAINT [UQ_ForecastPlanAccount] UNIQUE ([ForecastPlanId], [InstrumentId])
)
GO
