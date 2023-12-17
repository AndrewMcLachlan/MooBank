CREATE TABLE [dbo].[AccountAccountViewer]
(
    AccountId UNIQUEIDENTIFIER NOT NULL,
    AccountHolderId UNIQUEIDENTIFIER NOT NULL,
    AccountGroupId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_AccountAccountViewer PRIMARY KEY (AccountId, AccountHolderId),
    CONSTRAINT FK_AccountAccountViewer_Account FOREIGN KEY (AccountId) REFERENCES Account(AccountId),
    CONSTRAINT FK_AccountAccountViewer_AccountHolder FOREIGN KEY (AccountHolderId) REFERENCES AccountHolder(AccountHolderId),
    CONSTRAINT FK_AccountAccountViewer_AccountGroup FOREIGN KEY (AccountGroupId) REFERENCES [dbo].[AccountGroup](Id),
)
