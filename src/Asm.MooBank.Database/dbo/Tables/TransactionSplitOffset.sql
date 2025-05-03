CREATE TABLE [dbo].[TransactionSplitOffset] (
    TransactionSplitId UNIQUEIDENTIFIER NOT NULL,
    OffsetTransactionId UNIQUEIDENTIFIER NOT NULL,
    Amount DECIMAL(19,4) NOT NULL,
    CONSTRAINT [PK_TransactionSplitOffset] PRIMARY KEY CLUSTERED (TransactionSplitId, OffsetTransactionId),
    CONSTRAINT [FK_TransactionSplitOffset_TransactionSplit] FOREIGN KEY (TransactionSplitId) REFERENCES [TransactionSplit]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_TransactionSplitOffset_OffsetTransaction] FOREIGN KEY (OffsetTransactionId) REFERENCES [Transaction]([TransactionId]),
    --CONSTRAINT [CK_TransactionSplitOffset_Amount] CHECK([dbo].[CheckOffsetAmount](TransactionId, OffsetTransactionId, Amount) = 1)
)
