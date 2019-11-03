CREATE TABLE [dbo].[AccountAccountHolder]
(
	AccountId UNIQUEIDENTIFIER NOT NULL,
    AccountHolderId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_AccountAccountHolder PRIMARY KEY (AccountId, AccountHolderId),
    CONSTRAINT FK_AccountAccountHolder_Account FOREIGN KEY (AccountId) REFERENCES Account(AccountId),
    CONSTRAINT FK_AccountAccountHolder_AccountHolder FOREIGN KEY (AccountHolderId) REFERENCES AccountHolder(AccountHolderId),
)
