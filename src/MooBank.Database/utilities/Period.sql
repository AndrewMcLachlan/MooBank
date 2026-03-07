CREATE TABLE [utilities].[Period](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [BillId] [int] NOT NULL,
    [PeriodStart] [date] NOT NULL,
    [PeriodEnd] [date] NOT NULL,
    [DaysInclusive] AS (datediff(day,[PeriodStart],[PeriodEnd])+(1)) PERSISTED,
    [Days] AS (datediff(day,[PeriodStart],[PeriodEnd])) PERSISTED,
    CONSTRAINT [PK_Period] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Period_Bill] FOREIGN KEY([BillId])REFERENCES [utilities].[Bill] ([Id]),
)