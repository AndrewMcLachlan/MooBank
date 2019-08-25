CREATE TABLE [dbo].[AccountAccountHolder]
(
	AccountId UNIQUEIDENTIFIER NOT NULL,
    AccountHolderId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_AccountAccountHolder PRIMARY KEY (AccountId, AccountHolderId)
)
