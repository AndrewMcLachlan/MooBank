CREATE TABLE [dbo].[AccountHolderCard]
(
    [AccountHolderId] UNIQUEIDENTIFIER NOT NULL,
    [Last4Digits] SMALLINT NOT NULL,
    CONSTRAINT PK_AccountHolderCard PRIMARY KEY CLUSTERED (AccountHolderId, Last4Digits),
    CONSTRAINT FK_AccountHolderCard_AccountHolder FOREIGN KEY (AccountHolderId) REFERENCES [dbo].[AccountHolder](AccountHolderId)
)