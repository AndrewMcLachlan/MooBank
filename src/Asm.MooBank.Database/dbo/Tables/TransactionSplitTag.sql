CREATE TABLE [dbo].[TransactionSplitTag]
(
    [TransactionSplitId] UNIQUEIDENTIFIER NOT NULL,
    [TagId] INT NOT NULL,
    CONSTRAINT [PK_TransactionSplitTag] PRIMARY KEY CLUSTERED ([TransactionSplitId], [TagId]),
    CONSTRAINT [FK_TransactionSplitTag_TransactionSplit] FOREIGN KEY ([TransactionSplitId]) REFERENCES [TransactionSplit]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TransactionSplitTag_Tag] FOREIGN KEY ([TagId]) REFERENCES [Tag]([Id])
)
