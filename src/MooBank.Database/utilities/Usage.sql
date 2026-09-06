CREATE TABLE [utilities].[Usage](
    [Id] int IDENTITY(1,1) NOT NULL,
    [PeriodId] int NOT NULL,
    [PricePerUnit] decimal(7, 5) NOT NULL,
    [TotalUsage] decimal(7, 3) NOT NULL,
    [UsageTypeId] int NULL,
    -- Export is a credit, so it carries the rate as printed on the bill and the sign is applied
    -- here. Null counts as consumption, which is what every row was before export existed.
    [Cost] AS (CASE WHEN [UsageTypeId] = 2 THEN -([PricePerUnit]*[TotalUsage]) ELSE [PricePerUnit]*[TotalUsage] END),
    CONSTRAINT [PK_Usage] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Usage_Period] FOREIGN KEY([PeriodId]) REFERENCES [utilities].[Period] ([Id]),
    CONSTRAINT [FK_Usage_UsageType] FOREIGN KEY([UsageTypeId]) REFERENCES [utilities].[UsageType] ([Id])
)
