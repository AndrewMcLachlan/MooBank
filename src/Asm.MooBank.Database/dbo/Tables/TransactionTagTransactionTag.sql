CREATE TABLE [dbo].[TransactionTagTransactionTag]
(
	[PrimaryTransactionTagId] INT NOT NULL,
    [SecondaryTransactionTagId] INT NOT NULL,
    CONSTRAINT [PK_TransactionTagTransactionTag] PRIMARY KEY CLUSTERED (PrimaryTransactionTagId, SecondaryTransactionTagId),
    CONSTRAINT [FK_TransactionTag_TransactionTag_Primary] FOREIGN KEY (PrimaryTransactionTagId) REFERENCES [Tag]([Id]),
    CONSTRAINT [FK_TransactionTag_TransactionTag_Secondary] FOREIGN KEY (SecondaryTransactionTagId) REFERENCES [Tag]([Id]),
)
