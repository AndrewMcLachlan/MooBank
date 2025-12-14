CREATE TABLE [dbo].[TransactionSplitTag]
(
    [TransactionId] UNIQUEIDENTIFIER NOT NULL,
    [TransactionSplitId] INT NOT NULL,
    [TagId] INT NOT NULL,
    CONSTRAINT [PK_TransactionSplitTag] PRIMARY KEY CLUSTERED ([TransactionId], [TransactionSplitId], [TagId]),
    CONSTRAINT [FK_TransactionSplitTag_TransactionSplit] FOREIGN KEY ([TransactionId], [TransactionSplitId]) REFERENCES [TransactionSplit]([TransactionId], [Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TransactionSplitTag_Tag] FOREIGN KEY ([TagId]) REFERENCES [Tag]([Id])
)

GO

CREATE NONCLUSTERED INDEX [IX_TransactionSplitTag_TagId]
ON [dbo].[TransactionSplitTag] ([TagId]);
GO
