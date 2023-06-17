CREATE TABLE [dbo].[Budget]
(
    [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Budget_Id DEFAULT NEWID(),
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [Year] SMALLINT NOT NULL,
    CONSTRAINT [PK_Budget] PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT [FK_Budget_Account] FOREIGN KEY (AccountId) REFERENCES [Account](AccountId),
)

GO

CREATE UNIQUE INDEX [IX_Budget_AccountId_Year] ON [dbo].[Budget] ([AccountId], [Year])
