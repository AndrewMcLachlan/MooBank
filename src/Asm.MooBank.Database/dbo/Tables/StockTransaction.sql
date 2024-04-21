CREATE TABLE [dbo].[StockTransaction]
(
    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_StockTransaction_Id DEFAULT (NEWID()),
    AccountId UNIQUEIDENTIFIER NOT NULL,
    [AccountHolderId] UNIQUEIDENTIFIER NULL,
    [Description] VARCHAR(255) NULL,
    TransactionTypeId INT NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(12, 4) NOT NULL,
    Fees DECIMAL(12, 4) NOT NULL,
    TransactionDate DATETIMEOFFSET(0) NOT NULL,
    CONSTRAINT PK_StockTransaction PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_StockTransaction_StockHolding FOREIGN KEY (AccountId) REFERENCES StockHolding(AccountId),
    CONSTRAINT FK_StockTransaction_TransactionType FOREIGN KEY (TransactionTypeId) REFERENCES TransactionType(TransactionTypeId),
    CONSTRAINT [FK_StockTransaction_AccountHolder] FOREIGN KEY ([AccountHolderId]) REFERENCES [AccountHolder]([AccountHolderId]),
)