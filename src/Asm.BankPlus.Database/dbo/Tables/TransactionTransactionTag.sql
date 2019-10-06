CREATE TABLE [dbo].[TransactionTransactionTag]
(
	[TransactionId] UNIQUEIDENTIFIER NOT NULL,
    [TransactionTagId] INT NOT NULL,
    CONSTRAINT [PK_TransactionTransactionTag] PRIMARY KEY CLUSTERED (TransactionId, TransactionTagId),
    CONSTRAINT [FK_TransactionTransactionTag_Transaction] FOREIGN KEY (TransactionId) REFERENCES [Transaction](TransactionId),
    CONSTRAINT [FK_TransactionTransactionTag_TransactionTag] FOREIGN KEY (TransactionTagId) REFERENCES [TransactionTag](TransactionTagId)
)
