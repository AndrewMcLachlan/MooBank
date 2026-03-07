CREATE TABLE [utilities].[Bill](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [InvoiceNumber] VARCHAR(15) NULL,
    [IssueDate] DATE NOT NULL,
    [CurrentReading] INT NULL,
    [PreviousReading] INT NULL,
    [Total] AS ([CurrentReading]-[PreviousReading]) PERSISTED,
    [CostsIncludeGST] bit NULL CONSTRAINT [DF_Bill_CostsIncludeGST] DEFAULT (1),
    [Cost] AS [utilities].[TotalCost](Id),
    CONSTRAINT [PK_Bill] PRIMARY KEY CLUSTERED([Id] ASC),
    CONSTRAINT [FK_Bill_Account] FOREIGN KEY([AccountId]) REFERENCES [utilities].[Account] ([InstrumentId])
)