CREATE TABLE [dbo].[TransactionOffset] (
    TransactionId UNIQUEIDENTIFIER NOT NULL,
    OffsetTransactionId UNIQUEIDENTIFIER NOT NULL,
    Amount DECIMAL(19,4) NOT NULL,
    CONSTRAINT [PK_TransactionOffset] PRIMARY KEY CLUSTERED (TransactionId, OffsetTransactionId),
    CONSTRAINT [FK_TransactionOffset_Transaction] FOREIGN KEY (TransactionId) REFERENCES [Transaction]([TransactionId]),
    CONSTRAINT [FK_TransactionOffset_OffsetTransaction] FOREIGN KEY (OffsetTransactionId) REFERENCES [Transaction]([TransactionId]),
    CONSTRAINT [CK_Amount] CHECK([dbo].[CheckOffsetAmount](TransactionId, OffsetTransactionId, Amount) = 1)
)