CREATE TABLE [dbo].[TransactionAccount]
(
    [AccountId] UNIQUEIDENTIFIER NOT NULL,
    [LastTransaction] AS dbo.LastTransaction(AccountId),
    [CalculatedBalance] AS dbo.AccountBalance(AccountId),
    CONSTRAINT PK_TransactionAccount PRIMARY KEY CLUSTERED (AccountId),
    CONSTRAINT FK_TransactionAccount_Account FOREIGN KEY (AccountId) REFERENCES Account(AccountId),
)

GO
