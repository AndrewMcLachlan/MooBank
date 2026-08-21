CREATE TABLE [utilities].[ServiceCharge](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [PeriodId] int NOT NULL,
    [ChargePerDay] decimal(12, 5) NOT NULL,
    [ChargeTypeId] int NOT NULL CONSTRAINT [DF_ServiceCharge_ChargeType] DEFAULT (1),
    CONSTRAINT [PK_ServiceCharge] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ServiceCharge_Period] FOREIGN KEY([PeriodId]) REFERENCES [utilities].[Period] ([Id]),
    CONSTRAINT [FK_ServiceCharge_ChargeType] FOREIGN KEY([ChargeTypeId]) REFERENCES [utilities].[ChargeType] ([Id])
)
