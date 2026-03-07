CREATE TABLE [utilities].[Usage](
    [Id] int IDENTITY(1,1) NOT NULL,
    [PeriodId] int NOT NULL,
    [PricePerUnit] decimal(5, 5) NOT NULL,
    [TotalUsage] decimal(7, 3) NOT NULL,
    [Cost] AS ([PricePerUnit]*[TotalUsage]),
    CONSTRAINT [PK_Usage] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Usage_Period] FOREIGN KEY([PeriodId]) REFERENCES [utilities].[Period] ([Id])
)
