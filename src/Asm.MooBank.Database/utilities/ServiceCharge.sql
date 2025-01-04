CREATE TABLE [utilities].[ServiceCharge](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [PeriodId] int NOT NULL,
    [ChargePerDay] decimal(12, 5) NOT NULL,
    CONSTRAINT [PK_ServiceCharge] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ServiceCharge_Period] FOREIGN KEY([PeriodId]) REFERENCES [utilities].[Period] ([Id])
)
