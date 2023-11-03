CREATE TABLE [dbo].[TransactionSplit]
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_TransactionSplit_Id DEFAULT NEWID(),
    TransactionId UNIQUEIDENTIFIER NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,

    CONSTRAINT [PK_TransactionSplit] PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT [FK_TransactionSplit_Transaction] FOREIGN KEY (TransactionId) REFERENCES [Transaction](TransactionId),
    CONSTRAINT [CK_TransactionSplit_Amount] CHECK([dbo].[CheckSplitAmount](Id, TransactionId, Amount) = 1)
)
