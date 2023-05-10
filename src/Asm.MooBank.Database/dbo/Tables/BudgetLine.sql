CREATE TABLE [dbo].[BudgetLine]
(
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_BudgetLine_Id DEFAULT NEWID(),
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [TagId] INT NOT NULL,
    [Amount] DECIMAL(10,2) NOT NULL,
    [Month] SMALLINT NOT NULL CONSTRAINT DF_BudgetLine_Month DEFAULT (4095),
    [Income] BIT NOT NULL CONSTRAINT DF_BudgetLine_Income DEFAULT 0,
    CONSTRAINT [PK_BudgetLine] PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT [FK_BudgetLine_Account] FOREIGN KEY (AccountId) REFERENCES [Account](AccountId),
    CONSTRAINT [FK_BudgetLine_Tag] FOREIGN KEY (TagId) REFERENCES [TransactionTag](TransactionTagId)
)
GO

CREATE UNIQUE INDEX [IX_BudgetLine_TagId] ON [dbo].[BudgetLine] ([AccountId], [TagId])
