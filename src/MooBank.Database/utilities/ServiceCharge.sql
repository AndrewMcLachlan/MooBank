CREATE TABLE [utilities].[ServiceCharge](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [PeriodId] int NOT NULL,
    [ChargePerDay] decimal(12, 5) NOT NULL,
    -- Nullable only because a NOT NULL default is stamped, and its foreign key checked, before the
    -- post-deployment script can seed ChargeType. Populated for every row by that seed.
    [ChargeTypeId] int NULL,
    CONSTRAINT [PK_ServiceCharge] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ServiceCharge_Period] FOREIGN KEY([PeriodId]) REFERENCES [utilities].[Period] ([Id]),
    CONSTRAINT [FK_ServiceCharge_ChargeType] FOREIGN KEY([ChargeTypeId]) REFERENCES [utilities].[ChargeType] ([Id])
)
